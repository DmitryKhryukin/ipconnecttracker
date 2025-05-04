using IpConnectTracker.ReaderService.Api.Validation;

namespace IpConnectTracker.ReaderService.Api.UnitTests.Validation;

public class IpAddressPrefixValidatorTests
{
    [Theory]
    [InlineData("192")]
    [InlineData("192.168")]
    [InlineData("127.0.0.1")]
    [InlineData("8.8.8.8")]
    [InlineData("0.0.0.0")]
    [InlineData("10.0.0.")]
    [InlineData("255.255.255.")]
    public void IsValid_ValidIpv4Prefix_ShouldReturnTrue(string prefix)
    {
        var result = IpAddressPrefixValidator.IsValid(prefix);
        Assert.True(result);
    }

    [Theory]
    [InlineData("2001")]
    [InlineData("2001:db8")]
    [InlineData("fe80::")]
    [InlineData("2001:0db8:abcd:")]
    [InlineData("2001:db8:85a3::8a2e:370:7334")]
    [InlineData("::1")]
    public void IsValid_ValidIpv6Prefix_ShouldReturnTrue(string prefix)
    {
        var result = IpAddressPrefixValidator.IsValid(prefix);
        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    [InlineData("abc.def")]
    [InlineData("192.168.0.1.2")]
    [InlineData("hello:world")]
    public void IsValid_InvalidPrefix_ShouldReturnFalse(string prefix)
    {
        var result = IpAddressPrefixValidator.IsValid(prefix);
        Assert.False(result);
    }
}
