namespace Shared.Infrastructure.Kafka
{
    public interface IKafkaProducer
    {
        Task SendAsync<T>(string topic, T eventData);
    }
}
