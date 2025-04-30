using Microsoft.Extensions.Logging;

namespace IpConnectTracker.WriterService.IntegrationTests;

public class TestLogger : ILogger
{
    public List<string> Logs { get; }

    public TestLogger()
    {
        Logs = new List<string>();
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return NullScope.Instance;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Logs.Add(formatter(state, exception));
    }

    private class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose() { }
    }
}