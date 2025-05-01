using Dapper;
using IpConnectTracker.WriterService.DataAccess.Abstractions;
using IpConnectTracker.WriterService.DataAccess.PostgreSQL.Config;
using Microsoft.Extensions.Options;
using Npgsql;

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
        const string sql = "INSERT INTO user_connection_events (user_id, ip, timestamp) VALUES (@UserId, @Ip, @Timestamp);";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, 
            new
            {
                UserId = userId, 
                Ip = ipAddress, 
                Timestamp = timestamp
            });
    }
}