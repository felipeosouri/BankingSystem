using Microsoft.EntityFrameworkCore;
using Transaction.Domain.Repositories;
using Transaction.Infrastructure.Persistence;
using Transaction.Infrastructure.Persistence.Repositories;
using Transaction.Worker.Kafka;

var builder = Host.CreateApplicationBuilder(args);

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configuración de DbContext
builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Inyección de dependencias del dominio
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// Kafka consumer como BackgroundService
builder.Services.AddHostedService<TransactionStatusConsumer>();

var host = builder.Build();
host.Run();
