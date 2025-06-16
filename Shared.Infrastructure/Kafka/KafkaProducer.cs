using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Shared.Infrastructure.Kafka
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly KafkaSettings _settings;
        private readonly ILogger<KafkaProducer> _logger;

        public KafkaProducer(IOptions<KafkaSettings> options, ILogger<KafkaProducer> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task SendAsync<T>(string topic, T eventData)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = $"{_settings.Hostname}:{_settings.Port}"
            };

            using var producer = new ProducerBuilder<string, string>(config)
                .SetKeySerializer(Serializers.Utf8)
                .SetValueSerializer(Serializers.Utf8)
                .Build();

            var message = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = JsonSerializer.Serialize(eventData)
            };

            var result = await producer.ProduceAsync(topic, message);

            if (result.Status == PersistenceStatus.NotPersisted)
            {
                throw new Exception($"Failed to send message to Kafka topic '{topic}': {result.Message}");
            }

            _logger.LogInformation("Kafka: Event of type {EventType} sent to {Topic}", typeof(T).Name, topic);
        }
    }
}
