using Confluent.Kafka;
using Contracts.Events;
using System.Text.Json;
using Transaction.Domain.Enums;
using Transaction.Domain.Repositories;

namespace Transaction.Worker.Kafka;

public class TransactionStatusConsumer : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TransactionStatusConsumer> _logger;

    public TransactionStatusConsumer(
        IConfiguration config,
        IServiceScopeFactory scopeFactory,
        ILogger<TransactionStatusConsumer> logger)
    {
        _config = config;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var bootstrapServers = $"{_config["KafkaSettings:Hostname"]}:{_config["KafkaSettings:Port"]}";
            var topic = _config["KafkaSettings:StatusTopic"];

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = _config["Kafka:ConsumerConfig:GroupId"] ?? "transaction-status-consumer",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                AllowAutoCreateTopics = true,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();

            try
            {
                consumer.Subscribe(topic);
                _logger.LogInformation("TransactionStatusConsumer subscribed to topic {Topic}", topic);
            }
            catch (KafkaException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                _logger.LogWarning("Topic {Topic} not found. It may not be created yet. Kafka will retry.", topic);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(TimeSpan.FromSeconds(10));
                    if (cr?.Message == null) continue;

                    var evt = JsonSerializer.Deserialize<TransactionStatusEvent>(cr.Message.Value);
                    if (evt == null) continue;

                    using var scope = _scopeFactory.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();

                    var tx = await repository.GetByExternalIdAsync(evt.TransactionExternalId);
                    if (tx != null)
                    {
                        var status = evt.Status?.ToLowerInvariant() switch
                        {
                            "approved" => TransactionStatus.Approved,
                            "rejected" => TransactionStatus.Rejected,
                            _ => TransactionStatus.Pending
                        };

                        tx.UpdateStatus(status);
                        await repository.UpdateAsync(tx);

                        _logger.LogInformation("Transaction {TransactionId} updated to {Status}", tx.ExternalId, status);
                    }

                    consumer.Commit(cr);
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing transaction-status message");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error initializing TransactionStatusConsumer");
        }
    }
}
