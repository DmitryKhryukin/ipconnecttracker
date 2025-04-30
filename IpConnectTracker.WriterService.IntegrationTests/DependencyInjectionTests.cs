using IpConnectTracker.WriterService.Config;
using IpConnectTracker.WriterService.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IpConnectTracker.WriterService.IntegrationTests;

public class DependencyInjectionTests
{
    [Fact]
    public async Task Worker_ShouldBeSameInstance_ForWorkerAndRabbitMqListener()
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

                services.AddSingleton<Worker>();
                services.AddHostedService(sp => sp.GetRequiredService<Worker>());
                services.AddSingleton<RabbitMqListener>();
            })
            .Build();

        await host.StartAsync();

        var worker = host.Services.GetRequiredService<Worker>();
        var rabbit = host.Services.GetRequiredService<RabbitMqListener>();

        var workerField = typeof(RabbitMqListener)
            .GetField("_worker", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var rabbitWorker = workerField?.GetValue(rabbit);

        Assert.Same(worker, rabbitWorker);

        await host.StopAsync();
    }
}
