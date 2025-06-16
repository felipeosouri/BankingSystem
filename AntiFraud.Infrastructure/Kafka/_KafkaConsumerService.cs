using AntiFraud.Application.Services;
using Confluent.Kafka;
using Contracts.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Infrastructure.Kafka;
using System.Text.Json;

namespace AntiFraud.Infrastructure.Kafka
{
    public class _KafkaConsumerService : BackgroundService
    {
        private readonly ILogger<_KafkaConsumerService> _logger;
        private readonly IKafkaProducer _producer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConsumerConfig _consumerConfig;
        private readonly KafkaSettings _kafkaSettings;

        public _KafkaConsumerService(
            ILogger<_KafkaConsumerService> logger,
            IKafkaProducer producer,
            IServiceScopeFactory scopeFactory,
            IOptions<ConsumerConfig> consumerOptions,
            IOptions<KafkaSettings> kafkaOptions)
        {
            _logger = logger;
            _producer = producer;
            _scopeFactory = scopeFactory;
            _consumerConfig = consumerOptions.Value;
            _kafkaSettings = kafkaOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build(); 

            var inputTopic = "transaction-created";
            var outputTopic = _kafkaSettings.Topic; // e.g., transaction-status

            consumer.Subscribe(inputTopic);

            _logger.LogInformation("Kafka consumer started. Subscribed to topic: {Topic}", inputTopic);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(stoppingToken);
                    if (cr?.Message?.Value is null) continue;

                    var transaction = JsonSerializer.Deserialize<TransactionCreatedEvent>(cr.Message.Value);
                    if (transaction == null)
                    {
                        _logger.LogWarning("Received null or invalid transaction message.");
                        continue;
                    }

                    _logger.LogInformation("Processing transaction {Id}", transaction.TransactionExternalId);

                    // Crear un scope para obtener el servicio scoped
                    using var scope = _scopeFactory.CreateScope();
                    var evaluator = scope.ServiceProvider.GetRequiredService<IAntiFraudEvaluator>();
                    var status = await evaluator.EvaluateAsync(transaction);

                    var statusEvent = new TransactionStatusEvent
                    {
                        TransactionExternalId = transaction.TransactionExternalId,
                        Status = status
                    };

                    await _producer.SendAsync(outputTopic, statusEvent);
                    consumer.Commit(cr);

                    _logger.LogInformation("Published transaction status {Status} to {Topic}", status, outputTopic);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error in KafkaConsumerService");
                }
            }
        }
    }
}
