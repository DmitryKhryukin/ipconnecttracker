using System.Text;
using RabbitMQ.Client;

namespace IpConnectTracker.WriterService.Infrastructure;

public class RabbitMqConsumer : AsyncDefaultBasicConsumer
{
    private readonly ILogger _logger;
    private readonly MessageProcessorService _messageProcessorService;

    public RabbitMqConsumer(ILogger logger, IChannel channel, MessageProcessorService messsageProcessorService) : base(channel)
    {
        _logger = logger;
        _messageProcessorService = messsageProcessorService;
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

            await _messageProcessorService.EnqueueAsync(message, cancellationToken);
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