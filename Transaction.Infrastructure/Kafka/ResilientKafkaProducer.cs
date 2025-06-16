using Microsoft.Extensions.Logging;
using Polly;
using Shared.Infrastructure.Kafka;
using Shared.Infrastructure.Resilience;

namespace Transaction.Infrastructure.Kafka;

public class ResilientKafkaProducer : IKafkaProducer
{
    private readonly KafkaProducer _inner;
    private readonly ILogger<ResilientKafkaProducer> _logger;
    private readonly AsyncPolicy _kafkaPolicy;

    public ResilientKafkaProducer(KafkaProducer inner, ILogger<ResilientKafkaProducer> logger)
    {
        _inner = inner;
        _logger = logger;
        _kafkaPolicy = KafkaResilience.CreateKafkaPolicy();
    }

    public async Task SendAsync<T>(string topic, T eventData)
    {
        await _kafkaPolicy.ExecuteAsync(() => _inner.SendAsync(topic, eventData));
    }
}