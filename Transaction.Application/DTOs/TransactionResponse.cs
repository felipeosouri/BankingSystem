namespace Transaction.Application.DTOs;

public class TransactionResponse
{
    public Guid TransactionExternalId { get; set; }
    public DateTime CreatedAt { get; set; }
}