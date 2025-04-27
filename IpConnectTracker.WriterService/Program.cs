using IpConnectTracker.WriterService;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<Worker>();

var app = builder.Build();
app.MapGet("/health", () => "Healthy");
app.Run();