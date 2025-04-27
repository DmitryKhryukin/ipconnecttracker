using System.Threading.Channels;

namespace IpConnectTracker.WriterService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Channel<string> _channel;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;

        _channel = Channel.CreateBounded<string>(new BoundedChannelOptions(capacity: 10_000) // TODO: move to appsettings
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ValueTask EnqueueAsync(string message, CancellationToken cancellationToken = default)
    {
        return _channel.Writer.WriteAsync(message, cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var message in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                _logger.LogDebug("Processed message: {Message}", message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Writer exception: {ex.Message}");
        }
        finally
        {
            _logger.LogInformation("Worker stopped");
        }
    }
}