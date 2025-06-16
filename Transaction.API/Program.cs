using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Kafka;
using Shared.Infrastructure.Resilience;
using Transaction.Domain.Repositories;
using Transaction.Infrastructure.Kafka;
using Transaction.Infrastructure.Persistence;
using Transaction.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Controladores
builder.Services.AddControllers();

// 2. Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 3. EF Core
builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// 4. Repositorio de dominio
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// 5. Kafka Producer (resiliente)
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));
builder.Services.AddSingleton<KafkaProducer>(); // base
builder.Services.AddSingleton<IKafkaProducer, ResilientKafkaProducer>(); // decorador con Polly

// Construcción y ejecución
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
