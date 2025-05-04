using Dapper;
using IpConnectTracker.WriterService.DataAccess.Abstractions.Repository;
using IpConnectTracker.WriterService.DataAccess.PostgreSQL.Config;
using Microsoft.Extensions.Options;
using Npgsql;

namespace IpConnectTracker.WriterService.DataAccess.PostgreSQL.Repository;

public class PostgresConnectionEventWriteRepository : IConnectionEventWriteRepository
{
    private readonly string _connectionString;

    public PostgresConnectionEventWriteRepository(IOptions<PostgresWriteOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public async Task UpsertAsync(long userId, string ipAddress, DateTime timestamp, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO user_connection_events (user_id, ip_address, last_connected)
            VALUES (@UserId, @IpAddress::INET, @Timestamp)
            ON CONFLICT (user_id, ip_address)
            DO UPDATE SET last_connected = EXCLUDED.last_connected;
        ";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.ExecuteAsync(sql, 
            new
            {
                UserId = userId, 
                IpAddress = ipAddress, 
                Timestamp = timestamp
            });
    }
}