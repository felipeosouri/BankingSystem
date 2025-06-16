using AntiFraud.Worker.Kafka;
using AntiFraud.Worker.Services;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Kafka;
using Transaction.Domain.Repositories;
using Transaction.Infrastructure.Kafka;
using Transaction.Infrastructure.Persistence;
using Transaction.Infrastructure.Persistence.Repositories;

var builder = Host.CreateApplicationBuilder(args);

// Kafka config
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));
builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection("Kafka:ConsumerConfig"));

// Kafka producer
builder.Services.AddSingleton<KafkaProducer>();
builder.Services.AddSingleton<IKafkaProducer, ResilientKafkaProducer>();

builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Antifraud logic
builder.Services.AddScoped<IAntiFraudEvaluator, AntiFraudEvaluator>();

// Kafka consumer
builder.Services.AddHostedService<KafkaConsumerService>();

builder.Build().Run();
