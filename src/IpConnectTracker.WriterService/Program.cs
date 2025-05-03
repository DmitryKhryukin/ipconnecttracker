using IpConnectTracker.WriterService.Config;
using IpConnectTracker.WriterService.DataAccess.Abstractions.Repository;
using IpConnectTracker.WriterService.DataAccess.PostgreSQL.Config;
using IpConnectTracker.WriterService.DataAccess.PostgreSQL.Repository;
using IpConnectTracker.WriterService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("Rabbit"));
builder.Services.Configure<PostgresWriteOptions>(builder.Configuration.GetSection("Postgres"));

builder.Services.AddSingleton<MessageProcessorService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<MessageProcessorService>());
builder.Services.AddHostedService<RabbitMqListenerService>();

builder.Services.AddScoped<IConnectionEventRepository, PostgresConnectionEventRepository>();


var app = builder.Build();
app.MapGet("/health", () => "Healthy");
app.Run();