using IpConnectTracker.ReaderService.Api;
using IpConnectTracker.ReaderService.DataAccess.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConnectionEventReadRepository, MockConnectionEventReadRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapGet("/health", () => "Healthy");

app.Run();