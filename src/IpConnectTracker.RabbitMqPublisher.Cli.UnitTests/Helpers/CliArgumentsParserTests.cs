using IpConnectTracker.Cli.Helpers;

namespace IpConnectTracker.RabbitMqPublisher.Cli.UnitTests.Helpers;

public class CliArgumentsParserTests
{
    [Fact]
    public void ParseArgs_EmptyArgs_ReturnsDefaults()
    {
        var args = Array.Empty<string>();
        var result = CliArgumentsParser.ParseArgs(args);

        Assert.Equal(CliArgumentsParser.DefaultCount, result.Count);
        Assert.Equal(CliArgumentsParser.DefaultQueue, result.Queue);
        Assert.Equal(CliArgumentsParser.DefaultUserCount, result.UserCount);
    }

    [Fact]
    public void ParseArgs_WithCount_ParsesCorrectly()
    {
        var args = new[] { "--count", "42" };
        var result = CliArgumentsParser.ParseArgs(args);

        Assert.Equal(42, result.Count);
    }

    [Fact]
    public void ParseArgs_WithInvalidCount_UsesDefault()
    {
        var args = new[] { "--count", "invalid" };
        var result = CliArgumentsParser.ParseArgs(args);

        Assert.Equal(CliArgumentsParser.DefaultCount, result.Count);
    }

    [Fact]
    public void ParseArgs_CountWithoutValue_UsesDefault()
    {
        var args = new[] { "--count" };
        var result = CliArgumentsParser.ParseArgs(args);

        Assert.Equal(CliArgumentsParser.DefaultCount, result.Count);
    }

    [Fact]
    public void ParseArgs_WithQueue_ParsesCorrectly()
    {
        var args = new[] { "--queue", "custom_queue" };
        var result = CliArgumentsParser.ParseArgs(args);

        Assert.Equal("custom_queue", result.Queue);
    }

    [Fact]
    public void ParseArgs_QueueWithoutValue_UsesDefault()
    {
        var args = new[] { "--queue" };
        var result = CliArgumentsParser.ParseArgs(args);

        Assert.Equal(CliArgumentsParser.DefaultQueue, result.Queue);
    }

    [Fact]
    public void ParseArgs_WithUserCountFlag_SetsTrue()
    {
        var args = new[] { "--user-count", "50" };
        var result = CliArgumentsParser.ParseArgs(args);

        Assert.Equal(50, result.UserCount);;
    }

    [Fact]
    public void ParseArgs_MixedArguments_ParsesAllCorrectly()
    {
        var args = new[] { "--verbose", "--count", "500", "--queue", "logs", "--user-count", "1000", "--random" };
        var result = CliArgumentsParser.ParseArgs(args);
        
        Assert.Equal(500, result.Count);
        Assert.Equal("logs", result.Queue);
        Assert.Equal(1000, result.UserCount);
    }

    [Fact]
    public void ParseArgs_UnknownArgument_SkipsIt()
    {
        var args = new[] { "--unknown", "--count", "10" };
        var result = CliArgumentsParser.ParseArgs(args);

        Assert.Equal(10, result.Count);
    }
}