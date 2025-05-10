using System.Net;
using System.Text.Json;
using Dapper;
using IpConnectTracker.IntegrationTesting.Shared;
using IpConnectTracker.ReaderService.Api.IntegrationTests.Utils;
using IpConnectTracker.ReaderService.Api.Model;
using Npgsql;

namespace IpConnectTracker.ReaderService.Api.IntegrationTests.Controllers;

public class ConnectionEventsControllerTests : IClassFixture<PostgresWithFlywayFixture>
{
    private const long UnknownUserId = 9999;
    private const string UnknownIp = "8.8.8.8";
    private record TestConnection(long UserId, string IpAddress, DateTime LastConnected);
    
    private static readonly List<TestConnection> TestConnections = new List<TestConnection>
    {
        new(1001, "127.0.0.1", new DateTime(2025, 01, 01, 12, 00, 00, DateTimeKind.Utc)),
        new(1001, "192.168.0.2", new DateTime(2025, 01, 01, 11, 55, 00, DateTimeKind.Utc)),
        new(2002, "10.0.0.1", new DateTime(2025, 01, 01, 11, 50, 00, DateTimeKind.Utc))
    };

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

        foreach (var connection in TestConnections)
        {
            await conn.ExecuteAsync(@"
                INSERT INTO user_connection_events (user_id, ip_address, last_connected)
                VALUES (@UserId, CAST(@IpAddress AS inet), @LastConnected);", connection);
        }
    }

    [Fact]
    public async Task GetUserIps_UserConnected_ShouldReturnExpectedIps()
    {
        var userId = 1001;
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

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("The prefix field is required.", content);
    }
    
    [Fact]
    public async Task GetUsersByIpPrefix_PrefixHasInvalidFormat_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync("/api/connection-events/users/by-ip-prefix?prefix=asdf");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid ip address prefix.", content);
    }

    [Fact]
    public async Task GetUsersByIpPrefix_ValidPrefix_ShouldReturnExpectedUserIds()
    {
        var response = await _client.GetAsync("/api/connection-events/users/by-ip-prefix?prefix=192.168");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var users = JsonSerializer.Deserialize<List<long>>(content);

        Assert.Single(users);
        Assert.Contains(1001, users);
    }

    [Fact]
    public async Task GetUserLastConnection_UserDidNotConnect_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/connection-events/users/{UnknownUserId}/latest");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserLastConnection_UserConnected_ShouldReturnLatestConnection()
    {
        var expected = TestConnections.First(x => x.UserId == 1001 && x.IpAddress == "127.0.0.1");

        var response = await _client.GetAsync("/api/connection-events/users/1001/latest");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<UserConnectionDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        Assert.Equal(expected.IpAddress, dto.Ip);
        Assert.Equal(expected.LastConnected, dto.Timestamp);
    }

    [Fact]
    public async Task GetLatestByUserAndIp_IpIsEmpty_ShouldReturnValidationError()
    {
        var response = await _client.GetAsync("/api/connection-events/users/1001/latest-by-ip?ip=");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("The ip field is required", content);
    }

    [Fact]
    public async Task GetLatestByUserAndIp_UserDidNotConnectFromIp_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/connection-events/users/1001/latest-by-ip?ip={UnknownIp}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLatestByUserAndIp_UserConnectedFromIp_ShouldReturnTimestamp()
    {
        var expected = TestConnections.First(x => x.UserId == 1001 && x.IpAddress == "127.0.0.1");

        var response = await _client.GetAsync("/api/connection-events/users/1001/latest-by-ip?ip=127.0.0.1");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("timestamp", out var ts));
        Assert.Equal(expected.LastConnected, ts.GetDateTime());
    }
    
    [Fact]
    public async Task GetLatestByUserAndIp_InvalidIpFormat_ShouldReturnBadRequest()
    {
        var invalidIp = "not-an-ip";
        var url = $"/api/connection-events/users/1001/latest-by-ip?ip={invalidIp}";
        
        var response = await _client.GetAsync(url);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid IP address format", content);
    }

}
