using System.Text.Json;
using Dapper;
using IpConnectTracker.IntegrationTesting.Shared;
using IpConnectTracker.ReaderService.Api.IntegrationTests.Utils;
using IpConnectTracker.ReaderService.Api.Model;
using Npgsql;

namespace IpConnectTracker.ReaderService.Api.IntegrationTests.Controllers;

public class ConnectionEventsControllerTests : IClassFixture<PostgresWithFlywayFixture>
{
    private const long _testUserId1 = 1001;
    private const long _testUserId2 = 2002;
    private const long _unknownUserId = 9999;

    private const string _ip1 = "127.0.0.1";
    private const string _ip2 = "192.168.0.2";
    private const string _ip3 = "10.0.0.1";
    private const string _unknownIp = "8.8.8.8";

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

        await conn.ExecuteAsync(@$"
            INSERT INTO user_connection_events (user_id, ip_address, last_connected)
            VALUES
            ({_testUserId1}, '{_ip1}', NOW()),
            ({_testUserId1}, '{_ip2}', NOW() - interval '5 minutes'),
            ({_testUserId2}, '{_ip3}', NOW() - interval '10 minutes');
        ");
    }

    [Fact]
    public async Task GetUserIps_UserConnected_ShouldReturnExpectedIps()
    {
        var response = await _client.GetAsync($"/api/connection-events/users/{_testUserId1}/ips");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var ips = JsonSerializer.Deserialize<List<string>>(content)!;

        Assert.Equal(2, ips.Count);
        Assert.Contains(_ip1, ips);
        Assert.Contains(_ip2, ips);
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
        Assert.Contains(_testUserId1, users);
    }

    [Fact]
    public async Task GetUserLastConnection_UserDidNotConnect_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/connection-events/users/{_unknownUserId}/latest");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserLastConnection_UserConnected_ShouldReturnLatestConnection()
    {
        var response = await _client.GetAsync($"/api/connection-events/users/{_testUserId1}/latest");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var dto = JsonSerializer.Deserialize<UserConnectionDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        Assert.Equal(_ip1, dto.Ip);
        Assert.True(dto.ConnectedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task GetLatestByUserAndIp_IpIsEmpty_ShouldReturnValidationError()
    {
        var response = await _client.GetAsync($"/api/connection-events/users/{_testUserId1}/latest-by-ip?ip=");

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("The ip field is required", content);
    }

    [Fact]
    public async Task GetLatestByUserAndIp_UserDidNotConnectFromIp_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/connection-events/users/{_testUserId1}/latest-by-ip?ip={_unknownIp}");

        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetLatestByUserAndIp_UserConnectedFromIp_ShouldReturnTimestamp()
    {
        var response = await _client.GetAsync($"/api/connection-events/users/{_testUserId1}/latest-by-ip?ip={_ip1}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        Assert.True(doc.RootElement.TryGetProperty("timestamp", out var ts));
        Assert.True(ts.GetDateTime() <= DateTime.UtcNow);
    }
}
