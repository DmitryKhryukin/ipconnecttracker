using IpConnectTracker.WriterService;
using IpConnectTracker.WriterService.Config;
using IpConnectTracker.WriterService.DataAccess.Abstractions;
using IpConnectTracker.WriterService.DataAccess.PostgreSQL;
using IpConnectTracker.WriterService.DataAccess.PostgreSQL.Config;
using IpConnectTracker.WriterService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("Rabbit"));
builder.Services.Configure<PostgresOptions>(builder.Configuration.GetSection("Postgres"));

builder.Services.AddSingleton<MessageProcessorService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<MessageProcessorService>());
builder.Services.AddHostedService<RabbitMqListenerService>();

builder.Services.AddScoped<IConnectionEventRepository, PostgresConnectionEventRepository>();


var app = builder.Build();
app.MapGet("/health", () => "Healthy");
app.Run();