using System.Data;
using Dapper;
using IpConnectTracker.ReaderService.DataAccess.Abstractions.Repository;
using Npgsql;

namespace IpConnectTracker.ReaderService.DataAccess.PostgreSQL.Repository;

public class PostgresConnectionEventReadRepository : IConnectionEventReadRepository
{
    private readonly string _connectionString;

    public PostgresConnectionEventReadRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<IEnumerable<long>> GetUsersByIpPrefixAsync(string ipPrefix, int skip = 0, int take = 100, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = """
            SELECT DISTINCT user_id
            FROM user_connection_events
            WHERE ip_address::TEXT LIKE @IpPattern
            ORDER BY user_id
            OFFSET @Skip LIMIT @Take;
        """;

        return await connection.QueryAsync<long>(new CommandDefinition(
            sql,
            new { IpPattern = $"{ipPrefix}%", Skip = skip, Take = take },
            cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<string>> GetUserIpsAsync(long userId, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = """
            SELECT DISTINCT ip_address::text
            FROM user_connection_events
            WHERE user_id = @UserId
            ORDER BY ip_address;
        """;

        return await connection.QueryAsync<string>(new CommandDefinition(
            sql,
            new { UserId = userId },
            cancellationToken: cancellationToken));
    }

    public async Task<(string ip, DateTime timestamp)?> GetUserLastConnectionAsync(long userId, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = """
            SELECT ip_address::text, last_connected
            FROM user_connection_events
            WHERE user_id = @UserId
            ORDER BY last_connected DESC
            LIMIT 1;
        """;

        var result = await connection.QueryFirstOrDefaultAsync<(string, DateTime)?>(new CommandDefinition(
            sql,
            new { UserId = userId },
            cancellationToken: cancellationToken));
        
        return result;
    }

    public async Task<DateTime?> GetLastConnectionByUserAndIpAsync(long userId, string ip, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        var sql = """
            SELECT last_connected
            FROM user_connection_events
            WHERE user_id = @userId AND ip_address = @ip::inet
            ORDER BY last_connected DESC
            LIMIT 1;
        """;

        var result = await connection.ExecuteScalarAsync<DateTime?>(new CommandDefinition(
            sql,
            new { userId = userId, ip = ip },
            cancellationToken: cancellationToken));

        return result;
    }
}
