using MQTTnet;
using MQTTGatewayClient.Configuration;
using System.Buffers;

namespace MQTTGatewayClient.Services;

public class MqttService(
    ConnectionService connService,
    ILogger<MqttService> logger,
    GatewayOptions options) : BackgroundService
{
    private readonly ConnectionService _connService = connService;
    private readonly ILogger<MqttService> _logger = logger;
    private readonly GatewayOptions _options = options;

    private IMqttClient? _mqttClient;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunMqttConnectionAsync(stoppingToken);
            }
            catch (OperationCanceledException ex) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("MQTT service is stopping, error: " + ex.Message);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError("MQTT connection failed, error: " + ex.Message);
            }

            if(!stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning("MQTT connection lost. Reconnecting in 10 seconds....");

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    private async Task RunMqttConnectionAsync(CancellationToken stoppingToken)
    {
        var mqttFactory = new MqttClientFactory();

        var mqttClient = mqttFactory.CreateMqttClient();

        _mqttClient = mqttClient;
        
        try
        {
            _logger.LogInformation(
                "Connecting to MQTT broker {Host}:{Port}",
                _options.MqttHost,
                _options.MqttPort);

            var mqttOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(
                    _options.MqttHost,
                    _options.MqttPort)
                .WithCredentials(
                    _options.MqttUsername,
                    _options.MqttPassword)
                .Build();

            mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var payload = e.ApplicationMessage.Payload.ToArray();
                var topic = e.ApplicationMessage.Topic;

                _logger.LogInformation(
                    "MQTT message received on {Topic}: {Length} bytes",
                    topic,
                    payload.Length);

                try
                {
                    var route = _options.Routes.FirstOrDefault(r =>
                        MqttTopicMatcher.IsMatch(
                            topic,
                            r.MqttTopicFilter));

                    if (route == null)
                    {
                        _logger.LogWarning(
                            "No route found for MQTT topic: {Topic}",
                            topic);

                        return;
                    }

                    _logger.LogInformation(
                        "Forwarding MQTT message from topic {Topic} to Azure IoT Hub",
                        topic);

                    await _connService.SendAsync(
                        topic,
                        payload,
                        route.AzureConnectionString);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error forwarding MQTT message from topic {Topic}",
                        topic);
                }
            };

            await mqttClient.ConnectAsync(
                mqttOptions,
                stoppingToken);

            _logger.LogInformation(
                "Connected to MQTT broker.");

            var subscribeBuilder =
                new MqttClientSubscribeOptionsBuilder();

            foreach (var route in _options.Routes)
            {
                subscribeBuilder.WithTopicFilter(
                    route.MqttTopicFilter);

                _logger.LogInformation(
                    "Subscribing to MQTT topic filter: {Topic}",
                    route.MqttTopicFilter);
            }

            var subscribeOptions =
                subscribeBuilder.Build();

            await mqttClient.SubscribeAsync(
                subscribeOptions,
                stoppingToken);

            _logger.LogInformation(
                "Subscribed to {RouteCount} MQTT topic filters.",
                _options.Routes.Count);

            // Wait until the MQTT connection is lost
            // or the application is stopped.
            while (
                mqttClient.IsConnected &&
                !stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning(
                    "MQTT connection was lost.");
            }
        }
        finally
        {
            if (mqttClient.IsConnected)
            {
                try
                {
                    await mqttClient.DisconnectAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Error while disconnecting MQTT client.");
                }
            }

            mqttClient.Dispose();
            _mqttClient = null;
        }
    }
}