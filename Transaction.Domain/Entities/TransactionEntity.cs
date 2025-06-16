using Transaction.Domain.Enums;

namespace Transaction.Domain.Entities;

public class TransactionEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ExternalId { get; private set; } = Guid.NewGuid();
    public Guid SourceAccountId { get; private set; }
    public Guid TargetAccountId { get; private set; }
    public int TransferTypeId { get; private set; }
    public decimal Value { get; private set; }
    public TransactionStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public TransactionEntity(Guid sourceAccountId, Guid targetAccountId, int transferTypeId, decimal value)
    {
        SourceAccountId = sourceAccountId;
        TargetAccountId = targetAccountId;
        TransferTypeId = transferTypeId;
        Value = value;
        Status = TransactionStatus.Pending;
    }

    public void UpdateStatus(TransactionStatus newStatus)
    {
        Status = newStatus;
    }
}