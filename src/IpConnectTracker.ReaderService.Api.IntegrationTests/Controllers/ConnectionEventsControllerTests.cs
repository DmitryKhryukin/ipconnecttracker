using System.Text.Json;
using Dapper;
using IpConnectTracker.IntegrationTesting.Shared;
using IpConnectTracker.ReaderService.Api.IntegrationTests.Utils;
using IpConnectTracker.ReaderService.Api.Model;
using Npgsql;

namespace IpConnectTracker.ReaderService.Api.IntegrationTests.Controllers;

public class ConnectionEventsControllerTests : IClassFixture<PostgresWithFlywayFixture>
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
        
        await conn.ExecuteAsync("DELETE FROM user_connection_events;");
        
        await conn.ExecuteAsync(@"
            INSERT INTO user_connection_events (user_id, ip_address, last_connected)
            VALUES
            (1001, '127.0.0.1', NOW()),
            (1001, '192.168.0.2', NOW() - interval '5 minutes'),
            (2002, '10.0.0.1', NOW() - interval '10 minutes');
        ");
    }

    [Fact]
    public async Task GetUserIps_UserConnected_ShouldReturnExpectedIps()
    {
        long userId = 1001;
        var response = await _client.GetAsync($"/api/connection-events/users/{userId}/ips");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var ips = JsonSerializer.Deserialize<List<string>>(content)!;

        Assert.Equal(2, ips.Count);
        Assert.Contains("127.0.0.1", ips);
        Assert.Contains("192.168.0.2", ips);
    }

    [Fact]
    public async Task GetUsersByIpPrefix_PrefixIsEmpty_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync("/api/connection-events/users/by-ip-prefix?prefix=");

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("The prefix field is required.", content);
    }

    [Fact]
    public async Task GetUsersByIpPrefix_ValidPrefix_ShouldReturnExpectedUserIds()
    {
        var prefix = "192.168";
        var response = await _client.GetAsync($"/api/connection-events/users/by-ip-prefix?prefix={prefix}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<long>>(content)!;

        Assert.Single(users);
        Assert.Contains(1001, users);
    }

    [Fact]
    public async Task GetUserLastConnection_UserDidNotConnect_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/connection-events/users/9999/latest");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserLastConnection_UserConnected_ShouldReturnLatestConnection()
    {
        var userId = 1001;
        var response = await _client.GetAsync($"/api/connection-events/users/{userId}/latest");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<UserConnectionDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
        Assert.Equal("127.0.0.1", dto.Ip);
        Assert.True(dto.ConnectedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task GetLatestByUserAndIp_IpIsEmpty_ShouldReturnValidationError()
    {
        var response = await _client.GetAsync("/api/connection-events/users/1001/latest-by-ip?ip=");

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("The ip field is required", content);
    }

    [Fact]
    public async Task GetLatestByUserAndIp_UserDidNotConnectFromIp_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/connection-events/users/1001/latest-by-ip?ip=8.8.8.8");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLatestByUserAndIp_UserConnectedFromIp_ShouldReturnTimestamp()
    {
        var ip = "127.0.0.1";
        var response = await _client.GetAsync($"/api/connection-events/users/1001/latest-by-ip?ip={ip}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("timestamp", out var ts));
        Assert.True(ts.GetDateTime() <= DateTime.UtcNow);
    }
}
