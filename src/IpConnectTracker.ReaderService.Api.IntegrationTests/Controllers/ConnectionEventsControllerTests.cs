using System.Text.Json;
using Dapper;
using IpConnectTracker.IntegrationTesting.Shared;
using IpConnectTracker.ReaderService.Api.IntegrationTests.Utils;
using Npgsql;

namespace IpConnectTracker.ReaderService.Api.IntegrationTests.Controllers;

public class ConnectionEventsControllerTests: IClassFixture<PostgresWithFlywayFixture>
{
    private readonly HttpClient _client;
    
    public ConnectionEventsControllerTests(PostgresWithFlywayFixture fixture)
    {
        var factory = new CustomWebApplicationFactory(fixture.ConnectionString);
        _client = factory.CreateClient();
        
        SeedData(fixture.ConnectionString).GetAwaiter().GetResult();
    }

    private async Task SeedData(string connStr)
    {
        await using var conn = new NpgsqlConnection(connStr);
        await conn.ExecuteAsync(@"
            INSERT INTO user_connection_events (user_id, ip_address, last_connected)
            VALUES
            (1001, '127.0.0.1', NOW()),
            (1001, '192.168.0.2', NOW() - interval '5 minutes'),
            (2002, '10.0.0.1', NOW() - interval '10 minutes');
        ");
    }

    [Fact]
    public async Task GetUserIps_ReturnsExpectedIps()
    {
        var response = await _client.GetAsync("/api/connection-events/users/1001/ips");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var ips = JsonSerializer.Deserialize<List<string>>(content)!;
        
        Assert.True(ips.Count == 2);
        Assert.Contains("192.168.0.2", ips);
        Assert.Contains("127.0.0.1", ips);
    }
}