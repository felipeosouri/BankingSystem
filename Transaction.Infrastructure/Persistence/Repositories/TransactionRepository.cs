using Microsoft.EntityFrameworkCore;
using Polly;
using Transaction.Domain.Entities;
using Transaction.Domain.Repositories;

namespace Transaction.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly TransactionDbContext _db;

    public TransactionRepository(TransactionDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(TransactionEntity transaction)
    {
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync();
    }

    public async Task<TransactionEntity?> GetByExternalIdAsync(Guid externalId)
    {
        return await _db.Transactions.FirstOrDefaultAsync(t => t.ExternalId == externalId);
    }

    public async Task UpdateAsync(TransactionEntity transaction)
    {
        _db.Transactions.Update(transaction);
        await _db.SaveChangesAsync();
    }

    public async Task<decimal> GetDailyTotalByAccountAsync(Guid sourceAccountId, DateOnly date)
    {
        var start = date.ToDateTime(TimeOnly.MinValue);
        var end = date.ToDateTime(TimeOnly.MaxValue);

        return await _db.Transactions
            .Where(t => t.SourceAccountId == sourceAccountId && t.CreatedAt >= start && t.CreatedAt <= end)
            .SumAsync(t => (decimal?)t.Value) ?? 0m;
    }
}