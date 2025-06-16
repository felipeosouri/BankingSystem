using AntiFraud.Application.Services;
using AntiFraud.Infrastructure.Kafka;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Kafka;
using Transaction.Domain.Repositories;
using Transaction.Infrastructure.Kafka;
using Transaction.Infrastructure.Persistence;
using Transaction.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// 2. Configuración de Kafka
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));
builder.Services.Configure<ConsumerConfig>(builder.Configuration.GetSection("Kafka:ConsumerConfig"));

// 3. Kafka Producer (con resiliencia)
builder.Services.AddSingleton<KafkaProducer>();
builder.Services.AddSingleton<IKafkaProducer, ResilientKafkaProducer>();

// 4. EF Core: conexión a la base de datos de transacciones
builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// 5. Inyección del repositorio de transacciones
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// 6. Evaluador antifraude
builder.Services.AddScoped<IAntiFraudEvaluator, AntiFraudEvaluator>();

// 7. Background service que escucha desde Kafka
builder.Services.AddHostedService<_KafkaConsumerService>();

// 8. Opcional: Swagger + controladores (si quieres exponer endpoints)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
