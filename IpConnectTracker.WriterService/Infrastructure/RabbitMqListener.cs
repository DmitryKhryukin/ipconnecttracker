using IpConnectTracker.WriterService.Config;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace IpConnectTracker.WriterService.Infrastructure;

public sealed class RabbitMqListener : BackgroundService
{
    private readonly ILogger<RabbitMqListener> _logger;
    private readonly RabbitOptions _options;
    private readonly Worker _worker;
    
    private IConnection? _connection;
    private IChannel? _channel;
    private string? _consumerTag;

    public RabbitMqListener(
        ILogger<RabbitMqListener> logger,
        IOptions<RabbitOptions> options,
        Worker worker
        )
    {
        
        _options = options.Value;
        _worker = worker;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                ConsumerDispatchConcurrency = 1
            };

            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(options: null, cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(
                queue: _options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: stoppingToken);

            var consumer = new RabbitMqWorkerConsumer(_logger, _channel, _worker);

            _consumerTag = await _channel.BasicConsumeAsync(
                queue: _options.QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            _logger.LogInformation("RabbitMQ listener started on queue {Queue}", _options.QueueName);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("RabbitMQ listener stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ listener");
            throw;
        }
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping RabbitMQ listener...");

        try
        {
            if (_channel != null && _channel.IsOpen && _consumerTag != null)
            {
                await _channel.BasicCancelAsync(_consumerTag, cancellationToken: cancellationToken);
                await _channel.CloseAsync(cancellationToken);
                _channel.Dispose();
            }

            if (_connection != null && _connection.IsOpen)
            {
                await _connection.CloseAsync(cancellationToken);
                _connection.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during shutdown of RabbitMQ listener");
        }

        await base.StopAsync(cancellationToken);
    }
}
