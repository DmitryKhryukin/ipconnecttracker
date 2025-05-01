using IpConnectTracker.WriterService.DataAccess.Abstractions;
using IpConnectTracker.WriterService.DataAccess.PostgreSQL.Config;
using Microsoft.Extensions.Options;

namespace IpConnectTracker.WriterService.DataAccess.PostgreSQL;

public class PostgresConnectionEventRepository : IConnectionEventRepository
{
    private readonly string _connectionString;

    public PostgresConnectionEventRepository(IOptions<PostgresOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public async Task StoreAsync(long userId, string ipAddress, DateTime timestamp, CancellationToken cancellationToken)
    {
       //TODO: write to postgres
    }
}