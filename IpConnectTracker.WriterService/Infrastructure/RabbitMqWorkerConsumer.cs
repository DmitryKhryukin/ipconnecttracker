using System.Text;
using RabbitMQ.Client;

namespace IpConnectTracker.WriterService.Infrastructure;

public class RabbitMqWorkerConsumer : AsyncDefaultBasicConsumer
{
    private readonly ILogger _logger;
    private readonly Worker _worker;

    public RabbitMqWorkerConsumer(ILogger logger, IChannel channel, Worker worker) : base(channel)
    {
        _logger = logger;
        _worker = worker;
    }

    public override async Task HandleBasicDeliverAsync(
        string consumerTag,
        ulong deliveryTag,
        bool redelivered,
        string exchange,
        string routingKey,
        IReadOnlyBasicProperties properties,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var message = Encoding.UTF8.GetString(body.Span);
            _logger.LogDebug("Received message: {Message}", message);

            await _worker.EnqueueAsync(message, cancellationToken);
            await Channel.BasicAckAsync(deliveryTag, false, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            
            try
            {
                await Channel.BasicNackAsync(deliveryTag, false, true, cancellationToken);
            }
            catch (Exception nackEx)
            {
                _logger.LogError(nackEx, "Error during BasicNack");
            }
        }
    }
}