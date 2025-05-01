using IpConnectTracker.WriterService.Config;
using IpConnectTracker.WriterService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IpConnectTracker.WriterService.IntegrationTests;

public class DependencyInjectionTests
{
    [Fact]
    public async Task MessageProcessorService_ShouldBeSameInstance_ForMessageProcessorServiceAndRabbitMqListener()
    {
        var fakeOptions = new RabbitOptions
        {
            Host = "localhost",
            Port = 0000,
            QueueName = "test-queue",
            UserName = "fake",
            Password = "fake"
        };

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.Configure<RabbitOptions>(opts =>
                {
                    opts.Host = fakeOptions.Host;
                    opts.Port = fakeOptions.Port;
                    opts.QueueName = fakeOptions.QueueName;
                    opts.UserName = fakeOptions.UserName;
                    opts.Password = fakeOptions.Password;
                });

                services.AddSingleton<MessageProcessorService>();
                services.AddHostedService(sp => sp.GetRequiredService<MessageProcessorService>());
                services.AddSingleton<RabbitMqListenerService>();
            })
            .Build();

        await host.StartAsync();

        var messageProcessorService = host.Services.GetRequiredService<MessageProcessorService>();
        var rabbitMqListenerService = host.Services.GetRequiredService<RabbitMqListenerService>();

        var messageProcessorServiceField = typeof(RabbitMqListenerService)
            .GetField("_messageProcessorService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var rabbitMessageProcessorService = messageProcessorServiceField?.GetValue(rabbitMqListenerService);

        Assert.Same(messageProcessorService, rabbitMessageProcessorService);

        await host.StopAsync();
    }
}
