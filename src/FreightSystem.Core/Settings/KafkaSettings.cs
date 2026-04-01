namespace FreightSystem.Core.Settings
{
    public class KafkaSettings
    {
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string ClientId { get; set; } = "freight-system-api";
        public string? SaslUsername { get; set; }
        public string? SaslPassword { get; set; }
        public string SecurityProtocol { get; set; } = "plaintext";
        public string SaslMechanism { get; set; } = "PLAIN";
    }
}