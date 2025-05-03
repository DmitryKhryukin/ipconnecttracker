using System.Text;
using DotNet.Testcontainers.Builders;
using IpConnectTracker.WriterService.Config;
using IpConnectTracker.WriterService.DataAccess.Abstractions;
using IpConnectTracker.WriterService.DataAccess.Abstractions.Repository;
using IpConnectTracker.WriterService.Infrastructure;
using IpConnectTracker.WriterService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using RabbitMQ.Client;
using Testcontainers.RabbitMq;

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

        var mockConnectionEventRepository = new Mock<IConnectionEventRepository>();
        mockConnectionEventRepository.Setup(r => r.UpsertAsync(It.IsAny<long>(), 
                It.IsAny<string>(), 
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
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
                services.AddHostedService<RabbitMqListenerService>();
                
                services.AddScoped<IConnectionEventRepository>(_ => mockConnectionEventRepository.Object);
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

        long userId = 1234;
        string ipAddress = "127.0.0.1";
        var message = $"{userId},{ipAddress}";
        
        var messageBody = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: messageBody);

        var timeout = DateTime.UtcNow.AddSeconds(5);
        var logs = testLoggerProvider.Logger.Logs;

        while (!logs.Any(log => log.Contains("Processed message:")) && DateTime.UtcNow < timeout)
        {
            await Task.Delay(100);
        }

        mockConnectionEventRepository.Verify(repo => repo.UpsertAsync(
                It.Is<long>(x => x == userId),
                It.Is<string>(x => x == ipAddress),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
            
        await host.StopAsync();
    }
}
