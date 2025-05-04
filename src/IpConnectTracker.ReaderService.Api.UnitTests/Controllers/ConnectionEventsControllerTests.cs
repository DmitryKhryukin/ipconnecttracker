using IpConnectTracker.ReaderService.Api.Controllers;
using IpConnectTracker.ReaderService.Api.Model;
using IpConnectTracker.ReaderService.DataAccess.Abstractions.Repository;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IpConnectTracker.ReaderService.Api.UnitTests.Controllers;

public class ConnectionEventsControllerTests
{
    private readonly Mock<IConnectionEventReadRepository> _mockRepository;
    private readonly ConnectionEventsController _controller;

    public ConnectionEventsControllerTests()
    {
        _mockRepository = new Mock<IConnectionEventReadRepository>();
        _controller = new ConnectionEventsController(_mockRepository.Object);
    }

    [Fact]
    public async Task GetUsersByIpPrefix_PrefixIsEmpty_ShouldReturnBadRequest()
    {
        var result = await _controller.GetUsersByIpPrefix(" ");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid ip address prefix.", badRequest.Value);
    }
    
    [Fact]
    public async Task GetUsersByIpPrefix_PrefixIsInvalid_ShouldReturnBadRequest()
    {
        var result = await _controller.GetUsersByIpPrefix("invalid_prefix");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid ip address prefix.", badRequest.Value);
    }

    [Fact]
    public async Task GetUsersByIpPrefix_PrefixIsValid_ShouldReturnUsers()
    {
        var expectedIds = new List<long> { 1001, 1002 };
        var prefix = "192.168";
        
        _mockRepository.Setup(x => x.GetUsersByIpPrefixAsync(prefix, 0, 100, default))
                       .ReturnsAsync(expectedIds);

        var result = await _controller.GetUsersByIpPrefix(prefix);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedIds, okResult.Value);
    }

    [Fact]
    public async Task GetUserIps_ShouldReturnIps()
    {
        long userId = 1001;
        var expectedIps = new List<string> { "127.0.0.1", "192.168.0.1" };
        _mockRepository.Setup(r => r.GetUserIpsAsync(userId, default)).ReturnsAsync(expectedIps);

        var result = await _controller.GetUserIps(userId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedIps, okResult.Value);
    }

    [Fact]
    public async Task GetUserLastConnection_UserDidntConnect_ShouldReturnNotFound()
    {
        long userId = 1001;
        _mockRepository.Setup(x => x.GetUserLastConnectionAsync(userId, default))
            .ReturnsAsync((null as (string ip, DateTime timestamp)?));

        var result = await _controller.GetUserLastConnection(userId);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetUserLastConnection_UserConnected_ShouldReturnDto()
    {
        long userId = 1001;
        var ip = "192.168.0.1";
        var timestamp = DateTime.UtcNow;

        _mockRepository.Setup(x => x.GetUserLastConnectionAsync(userId, default))
            .ReturnsAsync((ip, timestamp));

        var result = await _controller.GetUserLastConnection(userId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<UserConnectionDto>(okResult.Value);
        Assert.Equal(ip, dto.Ip);
        Assert.Equal(timestamp, dto.ConnectedAt);
    }

    [Fact]
    public async Task GetLatestByUserAndIp_IpIsEmpty_ShouldReturnBadRequest()
    {
        var result = await _controller.GetLatestByUserAndIp(1, "");

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid IP address format.", badRequest.Value);
    }

    [Fact]
    public async Task GetLatestByUserAndIp_UserDidntConnectFromThisIp_ShouldReturnNotFound_()
    {
        _mockRepository.Setup(r => r.GetLastConnectionByUserAndIpAsync(1, "1.1.1.1", default))
                       .ReturnsAsync((DateTime?)null);

        var result = await _controller.GetLatestByUserAndIp(1, "1.1.1.1");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetLatestByUserAndIp_UserConnectedFromThisIp_ShouldReturnCorrectTimestamp()
    {
        long userId = 1001;
        var ipAddress = "1.1.1.1";
        var timestamp = DateTime.UtcNow;
        _mockRepository.Setup(r => r.GetLastConnectionByUserAndIpAsync(userId, ipAddress, default))
                       .ReturnsAsync(timestamp);

        var result = await _controller.GetLatestByUserAndIp(userId, ipAddress);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equivalent(new { timestamp }, okResult.Value);
    }
    
    [Theory]
    [InlineData("invalid format")]
    [InlineData("abc.asf.sadf.fasf")]
    [InlineData("::g")]
    [InlineData(null)]
    public async Task GetLatestByUserAndIp_InvalidOrEmptyIp_ShouldReturnBadRequest(string ipAddress)
    {
        var result = await _controller.GetLatestByUserAndIp(1, ipAddress);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid IP address format.", badRequest.Value);
    }
}
