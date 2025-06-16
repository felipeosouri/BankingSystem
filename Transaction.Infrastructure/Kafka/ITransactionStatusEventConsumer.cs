namespace Transaction.Infrastructure.Kafka.Interfaces
{
    public interface ITransactionStatusEventConsumer
    {
        Task ConsumeAsync(string topic, CancellationToken cancellationToken);
    }
}