using Microsoft.EntityFrameworkCore;

namespace Transaction.Infrastructure.Persistence;

public class TransactionDbContext : DbContext
{
    public TransactionDbContext(DbContextOptions<TransactionDbContext> options) : base(options)
    {
    }

    public DbSet<Domain.Entities.TransactionEntity> Transactions => Set<Domain.Entities.TransactionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entities.TransactionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ExternalId).IsUnique();
            entity.Property(e => e.SourceAccountId).IsRequired();
            entity.Property(e => e.TargetAccountId).IsRequired();
            entity.Property(e => e.TransferTypeId).IsRequired();
            entity.Property(e => e.Value).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}