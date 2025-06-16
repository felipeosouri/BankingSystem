namespace Shared.Infrastructure.Kafka
{
    public class KafkaSettings
    {
        public string Hostname { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string InputTopic { get; set; } = string.Empty;
    }
}
