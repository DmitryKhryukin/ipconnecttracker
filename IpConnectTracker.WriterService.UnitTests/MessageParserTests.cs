namespace IpConnectTracker.WriterService.UnitTests;

public class MessageParserTests
{
    [Theory]
    [InlineData("12345,127.0.0.1", 12345, "127.0.0.1")]
    [InlineData("0,::1", 0, "::1")]
    public void TryParse_ValidInput_ReturnsTrue(string input, long expectedUserId, string expectedIp)
    {
        var result = MessageParser.TryParse(input, out var userId, out var ip);

        Assert.True(result);
        Assert.Equal(expectedUserId, userId);
        Assert.Equal(expectedIp, ip);
    }

    [Theory]
    [InlineData("wronguserid,127.0.0.1")]
    [InlineData("12345")]
    [InlineData("12345,")]
    [InlineData(",127.0.0.1")]
    [InlineData("")]
    [InlineData(null)]
    public void TryParse_InvalidInput_ReturnsFalse(string input)
    {
        var result = MessageParser.TryParse(input, out var userId, out var ip);

        Assert.False(result);
    }
}