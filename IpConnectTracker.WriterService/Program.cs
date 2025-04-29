using IpConnectTracker.WriterService;
using IpConnectTracker.WriterService.Config;
using IpConnectTracker.WriterService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<Worker>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<Worker>());
builder.Services.AddHostedService<RabbitMqListener>();
builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("Rabbit"));

var app = builder.Build();
app.MapGet("/health", () => "Healthy");
app.Run();