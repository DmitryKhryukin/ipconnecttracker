using IpConnectTracker.ReaderService.DataAccess.Abstractions;
using IpConnectTracker.ReaderService.DataAccess.Abstractions.Repository;
using Microsoft.AspNetCore.Mvc;

namespace IpConnectTracker.ReaderService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        if (string.IsNullOrWhiteSpace(prefix))
            return BadRequest("Prefix cannot be empty.");
        
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
        var result = await _repository.GetUserLastConnectionAsync(userId);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("ips/{ip}/last-connection")]
    public async Task<IActionResult> GetLastConnectionByIp(string ip)
    {
        var timestamp = await _repository.GetLastConnectionByIpAsync(ip);
        return timestamp is null ? NotFound() : Ok(new { timestamp });
    }
}