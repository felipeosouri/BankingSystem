using AntiFraud.Application.Services;
using Contracts.Events;
using Microsoft.Extensions.Logging;
using Transaction.Domain.Repositories;

public class AntiFraudEvaluator : IAntiFraudEvaluator
{
    private readonly ILogger<AntiFraudEvaluator> _logger;
    private readonly ITransactionRepository _repository;

    public AntiFraudEvaluator(ILogger<AntiFraudEvaluator> logger, ITransactionRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<string> EvaluateAsync(TransactionCreatedEvent evt)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var accumulated = await _repository.GetDailyTotalByAccountAsync(evt.SourceAccountId, today);
        var newTotal = accumulated + evt.Value;

        _logger.LogInformation("Evaluating transaction {ExternalId}. Value: {Value}, DailyTotalWithCurrent: {Total}",
            evt.TransactionExternalId, evt.Value, newTotal);

        if (evt.Value > 2000 || newTotal > 20000)
        {
            return "Rejected";
        }

        return "Approved";
    }
}