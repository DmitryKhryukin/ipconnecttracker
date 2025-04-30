using Microsoft.Extensions.Logging;

namespace IpConnectTracker.WriterService.IntegrationTests;

public class TestLoggerProvider : ILoggerProvider
{
    private readonly TestLogger _logger;
    
    public TestLogger Logger => _logger;

    public TestLoggerProvider()
    {
        _logger = new TestLogger();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _logger;
    }

    public void Dispose() { }
}
