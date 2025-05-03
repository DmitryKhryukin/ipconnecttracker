using Dapper;
using IpConnectTracker.IntegrationTesting.Shared;
using Npgsql;

namespace IpConnectTracker.WriterService.IntegrationTests;

public class PostgresSchemaTests : PostgresWithFlywayFixture
{
    [Fact(Timeout = 60_000)]
    public async Task RunMigrationScripts_Table_user_connection_events_Should_Exist()
    {
        var connectionString = $"Host={PostgresContainer.Hostname};Port={PostgresContainer.GetMappedPublicPort(5432)};Database=iptracker;Username=app;Password=secret";
        
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var result = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'user_connection_events';");

        Assert.Equal(1, result);
    }
}