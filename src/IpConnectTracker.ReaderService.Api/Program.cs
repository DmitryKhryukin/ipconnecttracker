using IpConnectTracker.ReaderService.Api;
using IpConnectTracker.ReaderService.DataAccess.Abstractions.Repository;
using IpConnectTracker.ReaderService.DataAccess.PostgreSQL.Config;
using IpConnectTracker.ReaderService.DataAccess.PostgreSQL.Repository;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddSingleton<IConnectionEventReadRepository, MockConnectionEventReadRepository>();

builder.Services.Configure<PostgresReadOptions>(
    builder.Configuration.GetSection("Postgres"));

builder.Services.AddScoped<IConnectionEventReadRepository>(sp =>
{
    var options = sp.GetRequiredService<IOptions<PostgresReadOptions>>().Value;
    return new PostgresConnectionEventReadRepository(options.ConnectionString);
});

var app = builder.Build();

app.UseMiddleware<IpConnectTracker.ReaderService.Api.Middleware.RequestTimeMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapGet("/health", () => "Healthy");

app.Run();