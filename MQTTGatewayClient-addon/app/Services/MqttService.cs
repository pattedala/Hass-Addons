using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTGatewayClient.Configuration;
using MQTTGatewayClient.Models;

namespace MQTTGatewayClient.Services;

public class MqttService : BackgroundService
{
    private readonly IoTHubService _iotHubService;
    private readonly ILogger<MqttService> _logger;
    private readonly GatewayOptions _options;

    private IMqttClient? _mqttClient;

    public MqttService(
        IoTHubService iotHubService,
        ILogger<MqttService> logger,
        GatewayOptions options)
    {
        _iotHubService = iotHubService;
        _logger = logger;
        _options = options;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var mqttFactory = new MqttClientFactory();

        var mqttClient = mqttFactory.CreateMqttClient();

        _mqttClient = mqttClient;

        var mqttHost = _options.MqttHost;

        if (string.IsNullOrWhiteSpace(mqttHost))
        {
            throw new InvalidOperationException(
                "MQTT host is not configured.");
        }

        var mqttPort = _options.MqttPort;

        var mqttUsername = _options.MqttUsername;

        if (string.IsNullOrWhiteSpace(mqttUsername))
        {
            throw new InvalidOperationException(
                "MQTT username is not configured.");
        }

        var mqttPassword = _options.MqttPassword;

        if (string.IsNullOrWhiteSpace(mqttPassword))
        {
            throw new InvalidOperationException(
                "MQTT password is not configured.");
        }

        var mqttTopic = _options.MqttTopic;

        if (string.IsNullOrWhiteSpace(mqttTopic))
        {
            throw new InvalidOperationException(
                "MQTT topic is not configured.");
        }

        _logger.LogInformation(
            "Connecting to MQTT broker {Host}:{Port}",
            mqttHost,
            mqttPort);

        var mqttOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(mqttHost, mqttPort)
            .WithCredentials(
                mqttUsername,
                mqttPassword)
            .Build();

        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var messageJson =
                Encoding.UTF8.GetString(
                    e.ApplicationMessage.Payload);

            _logger.LogInformation(
                "MQTT message received on {Topic}: {Message}",
                e.ApplicationMessage.Topic,
                messageJson);

            try
            {
                var message =
                    JsonSerializer.Deserialize<ModellMessage>(
                        messageJson);

                if (message != null)
                {
                    await _iotHubService.SendAsync(
                        message);
                }
                else
                {
                    _logger.LogWarning(
                        "Unable to deserialize MQTT message.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing MQTT message.");
            }
        };

        try
        {
            await mqttClient.ConnectAsync(
                mqttOptions,
                stoppingToken);

            _logger.LogInformation(
                "Connected to MQTT broker.");

            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(mqttTopic)
                .Build();

            await mqttClient.SubscribeAsync(
                subscribeOptions,
                stoppingToken);

            _logger.LogInformation(
                "Subscribed to MQTT topic: {Topic}",
                mqttTopic);

            // Keep the BackgroundService alive while MQTT is connected.
            await Task.Delay(
                Timeout.Infinite,
                stoppingToken);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "MQTT service is stopping.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "MQTT service failed.");
            throw;
        }
        finally
        {
            if (mqttClient.IsConnected)
            {
                await mqttClient.DisconnectAsync();
            }

            mqttClient.Dispose();

            _mqttClient = null;
        }
    }
}