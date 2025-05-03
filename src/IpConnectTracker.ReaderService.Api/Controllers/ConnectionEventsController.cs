using IpConnectTracker.ReaderService.Api.Model;
using IpConnectTracker.ReaderService.DataAccess.Abstractions.Repository;
using Microsoft.AspNetCore.Mvc;

namespace IpConnectTracker.ReaderService.Api.Controllers;

[ApiController]
[Route("api/connection-events")]
public class ConnectionEventsController : ControllerBase
{
    private readonly IConnectionEventReadRepository _repository;

    public ConnectionEventsController(IConnectionEventReadRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("users/by-ip-prefix")]
    public async Task<IActionResult> GetUsersByIpPrefix([FromQuery] string prefix,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
        var users = await _repository.GetUsersByIpPrefixAsync(prefix, skip, take);
        return Ok(users);
    }

    [HttpGet("users/{userId}/ips")]
    public async Task<IActionResult> GetUserIps(long userId)
    {
        var ips = await _repository.GetUserIpsAsync(userId);
        return Ok(ips);
    }

    [HttpGet("users/{userId}/latest")]
    public async Task<IActionResult> GetUserLastConnection(long userId)
    {
        var latestConnection = await _repository.GetUserLastConnectionAsync(userId);

        if (latestConnection is null)
        {
            return NotFound();
        }

        var result = new UserConnectionDto(latestConnection.Value.ip, latestConnection.Value.timestamp);

        return Ok(result);
    }

    [HttpGet("users/{userId}/latest-by-ip")]
    public async Task<IActionResult> GetLatestByUserAndIp(long userId, [FromQuery(Name = "ip")] string ip)
    {
        var timestamp = await _repository.GetLastConnectionByUserAndIpAsync(userId, ip);
        return timestamp is null
            ? NotFound()
            : Ok(new { timestamp });
    }
}