using Polly;

namespace Shared.Infrastructure.Resilience;

public static class KafkaResilience
{
    public static AsyncPolicy CreateKafkaPolicy()
    {
        var retry = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, time) =>
                {
                    Console.WriteLine($"[Kafka Retry] Retrying due to: {exception.Message}");
                });

        var circuitBreaker = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(20),
                onBreak: (ex, breakDelay) =>
                {
                    Console.WriteLine($"[Kafka CircuitBreaker] Open due to: {ex.Message}");
                },
                onReset: () => Console.WriteLine("[Kafka CircuitBreaker] Closed again"),
                onHalfOpen: () => Console.WriteLine("[Kafka CircuitBreaker] Half-open: next call is a test"));

        return Policy.WrapAsync(retry, circuitBreaker);
    }
}