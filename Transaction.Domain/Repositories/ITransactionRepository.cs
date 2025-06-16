using Transaction.Domain.Entities;

namespace Transaction.Domain.Repositories;

public interface ITransactionRepository
{
    Task<TransactionEntity> GetByExternalIdAsync(Guid externalId);
    Task AddAsync(TransactionEntity transaction);
    Task UpdateAsync(TransactionEntity transaction);
    Task<decimal> GetDailyTotalByAccountAsync(Guid sourceAccountId, DateOnly date);
}