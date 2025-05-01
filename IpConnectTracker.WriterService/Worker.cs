using System.Threading.Channels;
using IpConnectTracker.WriterService.DataAccess.Abstractions;

namespace IpConnectTracker.WriterService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly Channel<string> _channel;
    private readonly IConnectionEventRepository _repository;

    public Worker(ILogger<Worker> logger, IConnectionEventRepository repository)
    {
        _logger = logger;
        _repository = repository;

        _channel = Channel.CreateBounded<string>(
            new BoundedChannelOptions(capacity: 10_000) // TODO: move to appsettings
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

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var message in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                if (!MessageParser.TryParse(message, out var userId, out var ip))
                {
                    _logger.LogWarning("Invalid message format: {Message}", message);
                    return;
                }

                await _repository.StoreAsync(userId, ip, DateTime.UtcNow, cancellationToken);
                _logger.LogDebug("Stored event for user {UserId} with IP {IP}", userId, ip);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while processing message: {Message}", ex.Message);
        }
    }
}