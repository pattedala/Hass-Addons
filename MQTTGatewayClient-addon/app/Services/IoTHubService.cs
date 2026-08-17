using Microsoft.Azure.Devices.Client;
using System.Text;
using System.Text.Json;
using MQTTGatewayClient.Configuration;
using MQTTGatewayClient.Models;

namespace MQTTGatewayClient.Services
{
    public class IoTHubService
    {
        private readonly string _deviceConnectionString;
        private readonly ILogger<IoTHubService> _logger;
        private readonly DeviceClient _client;

        public IoTHubService(GatewayOptions options, ILogger<IoTHubService> logger)
        {
            _deviceConnectionString = options.IoTHubConnectionString ?? throw new InvalidOperationException("IoT Hub connection string is not configured.");
            _logger = logger;
            _client = DeviceClient.CreateFromConnectionString(_deviceConnectionString, TransportType.Mqtt);
        }

        public async Task SendAsync(ModellMessage message)
        {
            var messageJson = JsonSerializer.Serialize(message);
            var telemetry = new Message(Encoding.UTF8.GetBytes(messageJson));

            telemetry.ContentType = "application/json";
            telemetry.ContentEncoding = "utf-8";

            await _client.SendEventAsync(telemetry);
        }
    }
}