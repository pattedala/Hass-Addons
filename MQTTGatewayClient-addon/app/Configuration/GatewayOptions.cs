using System.Text.Json.Serialization;
namespace MQTTGatewayClient.Configuration
{
    public class GatewayOptions
    {
        [JsonPropertyName("mqtt_host")]
        public string MqttHost { get; set; } = "core-mosquitto";
        [JsonPropertyName("mqtt_port")]
        public int MqttPort { get; set; } = 1883;
        [JsonPropertyName("mqtt_username")]
        public string? MqttUsername { get; set; }
        [JsonPropertyName("mqtt_password")]
        public string? MqttPassword { get; set; }
        [JsonPropertyName("mqtt_topic")]
        public string MqttTopic { get; set; } = string.Empty;
        [JsonPropertyName("iothub_connection_string")]
        public string IoTHubConnectionString { get; set; } = string.Empty;
    }
}