using System.Text;
using DotNet.Testcontainers.Builders;
using IpConnectTracker.WriterService;
using IpConnectTracker.WriterService.Config;
using IpConnectTracker.WriterService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;
using Xunit;

namespace IpConnectTracker.WriterService.IntegrationTests;

public class RabbitMqContainerTests : IAsyncLifetime
{
    private const int TestPort = 5673;
    private const int DefaultRabbitMqContainerPort = 5672;
    private const string UserName = "guest";
    private const string Password = "guest";
    
    private readonly RabbitMqContainer _rabbitMqContainer;

    public RabbitMqContainerTests()
    {
        _rabbitMqContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management") // TODO: use the latest version?
            .WithPortBinding(TestPort, DefaultRabbitMqContainerPort)
            .WithUsername("guest")
            .WithPassword("guest")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(DefaultRabbitMqContainerPort))
            .Build();
    }

    public async Task InitializeAsync() => await _rabbitMqContainer.StartAsync();
    public async Task DisposeAsync() => await _rabbitMqContainer.DisposeAsync();

    [Fact(Timeout = 60_000)]
    public async Task MessageProcessorService_Should_Process_Message_From_RabbitMq_Container()
    {
        const string queueName = "ip_connects_test";
        const int rabbitMqPort = 5672;
        var testLoggerProvider = new TestLoggerProvider();

        var host = Host.CreateDefaultBuilder()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddProvider(testLoggerProvider);
            })
            .ConfigureServices(services =>
            {
                services.Configure<RabbitOptions>(opts =>
                {
                    opts.Host = _rabbitMqContainer.Hostname;
                    opts.Port = _rabbitMqContainer.GetMappedPublicPort(rabbitMqPort);
                    opts.UserName = UserName;
                    opts.Password = Password;
                    opts.QueueName = queueName;
                });

                services.AddSingleton<MessageProcessorService>();
                services.AddHostedService(sp => sp.GetRequiredService<MessageProcessorService>());
                services.AddHostedService<RabbitMqListener>();
            })
            .Build();

        await host.StartAsync();

        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqContainer.Hostname,
            Port = _rabbitMqContainer.GetMappedPublicPort(rabbitMqPort),
            UserName = UserName,
            Password = Password,
            ConsumerDispatchConcurrency = 1
        };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false);

        var userName = "testUser";
        var message = $"{userName},127.0.0.1";
        
        var messageBody = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: messageBody);

        var timeout = DateTime.UtcNow.AddSeconds(5);
        var logs = testLoggerProvider.Logger.Logs;

        while (!logs.Any(log => log.Contains("Processed message:")) && DateTime.UtcNow < timeout)
        {
            await Task.Delay(100);
        }

        Assert.Contains(logs, log => log.Contains("Processed message:") && log.Contains(userName));

        await host.StopAsync();
    }

}
