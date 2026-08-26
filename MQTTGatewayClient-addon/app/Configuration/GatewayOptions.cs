using System.Text.Json.Serialization;

namespace MQTTGatewayClient.Configuration;

public class GatewayOptions
{
    [JsonPropertyName("mqtt_host")]
    public string MqttHost { get; set; } = string.Empty;

    [JsonPropertyName("mqtt_port")]
    public int MqttPort { get; set; } 

    [JsonPropertyName("mqtt_username")]
    public string? MqttUsername { get; set; }

    [JsonPropertyName("mqtt_password")]
    public string? MqttPassword { get; set; }

    [JsonPropertyName("routes")]
    public List<MqttRoute> Routes { get; set; } = new();
}

public class MqttRoute
{
    [JsonPropertyName("mqtt_topic")]
    public string MqttTopicFilter  { get; set; } = string.Empty;

    [JsonPropertyName("azure_connection_string")]
    public string AzureConnectionString { get; set; } = string.Empty;
}