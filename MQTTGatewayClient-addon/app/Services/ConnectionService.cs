using Microsoft.Azure.Devices.Client;
using System.Collections.Concurrent;

namespace MQTTGatewayClient.Services
{
    public class ConnectionService(ILogger<ConnectionService> logger) : IDisposable
    {
        private readonly ILogger<ConnectionService> _logger = logger;
        private readonly ConcurrentDictionary<string, DeviceClient> _clients = [];

        public async Task SendAsync(string topic, byte[] payload, string azureConnectionString)
        {

            if(string.IsNullOrWhiteSpace(azureConnectionString))
            {
                throw new InvalidOperationException("Azure Connection string is not configured");
            }

            if(payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var client = _clients.GetOrAdd(azureConnectionString, connectionString =>
            {
                _logger.LogInformation("Creating Azure client for new connection");

                return DeviceClient.CreateFromConnectionString(connectionString, TransportType.Mqtt);
            });

            var message = new Message(payload)
            {
                ContentType = "application/octet-stream"
            };

            message.Properties["mqttTopic"] = topic;

            _logger.LogInformation("Sending {Length} bytes from MQTT topic {Topic} to Azure", payload.Length, topic);

            // await client.SendEventAsync(message);
            await SendWithRetryAsync(client, message, topic);
        }

        private async Task SendWithRetryAsync(
        DeviceClient client,
        Message message,
        string topic)
        {
            const int maxAttempts = 10;

            for(var attempt =1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await client.SendEventAsync(message);

                    _logger.LogDebug("Successfully sent MQTT message from topic {Topic} to Azure", topic);

                    return;
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    _logger.LogWarning(ex, "Failed to send MQTT message from topic {Topic} to Azure. " +
                    "Retrying in 10 seconds (attepmt {Attempt}/{MaxAttempts}).", topic, attempt, maxAttempts);

                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            }

            await client.SendEventAsync(message);
        }

        public void Dispose()
        {
            foreach(var client in _clients.Values)
            {
                client.Dispose();
            }

            _clients.Clear();
        }
    }
}