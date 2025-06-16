using Contracts.Events;

namespace AntiFraud.Worker.Services;

public interface IAntiFraudEvaluator
{
    Task<string> EvaluateAsync(TransactionCreatedEvent evt);
}