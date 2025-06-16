using Contracts.Events;

namespace AntiFraud.Application.Services;

public interface IAntiFraudEvaluator
{
    Task<string> EvaluateAsync(TransactionCreatedEvent evt);
}