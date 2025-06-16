namespace Contracts.Events;

public class TransactionStatusEvent
{
    public Guid TransactionExternalId { get; set; }
    public string Status { get; set; }
}