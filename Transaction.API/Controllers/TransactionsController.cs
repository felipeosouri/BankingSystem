using Contracts.Events;
using Microsoft.AspNetCore.Mvc;
using Shared.Infrastructure.Kafka;
using Transaction.Application.DTOs;
using Transaction.Domain.Repositories;

namespace Transaction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionRepository _repository;
    private readonly IKafkaProducer _kafka;
    private readonly IConfiguration _config;

    public TransactionsController(ITransactionRepository repository, IKafkaProducer kafka, IConfiguration config)
    {
        _repository = repository;
        _kafka = kafka;
        _config = config;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
    {
        var transaction = new Domain.Entities.TransactionEntity(
            request.SourceAccountId,
            request.TargetAccountId,
            request.TransferTypeId,
            request.Value
        );

        await _repository.AddAsync(transaction);

        var evt = new TransactionCreatedEvent
        {
            TransactionExternalId = transaction.ExternalId,
            SourceAccountId = transaction.SourceAccountId,
            TargetAccountId = transaction.TargetAccountId,
            TransferTypeId = transaction.TransferTypeId,
            Value = transaction.Value
        };

        var topic = _config["KafkaSettings:Topic"];
        await _kafka.SendAsync(topic, evt);

        return CreatedAtAction(nameof(GetById), new { transactionExternalId = transaction.ExternalId }, new TransactionResponse
        {
            TransactionExternalId = transaction.ExternalId,
            CreatedAt = transaction.CreatedAt
        });
    }

    [HttpGet("{transactionExternalId}")]
    public async Task<IActionResult> GetById(Guid transactionExternalId)
    {
        var tx = await _repository.GetByExternalIdAsync(transactionExternalId);
        if (tx == null) return NotFound();

        return Ok(new TransactionResponse
        {
            TransactionExternalId = tx.ExternalId,
            CreatedAt = tx.CreatedAt
        });
    }
}
