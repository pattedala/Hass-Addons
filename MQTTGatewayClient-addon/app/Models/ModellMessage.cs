namespace MQTTGatewayClient.Models
{
    public class ModellMessage
    {
        public string DeviceId { get; set; } = "";
        public double Temperature { get; set; }
        public DateTime Timestamp { get; set; }
    }
}