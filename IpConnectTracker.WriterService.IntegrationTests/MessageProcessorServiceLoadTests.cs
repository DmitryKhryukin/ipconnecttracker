using IpConnectTracker.WriterService.DataAccess.Abstractions;
using IpConnectTracker.WriterService.DataAccess.Abstractions.Repository;
using IpConnectTracker.WriterService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NBomber.CSharp;

namespace IpConnectTracker.WriterService.IntegrationTests;

public class MessageProcessorServiceLoadTests
{
    [Fact(Timeout = 60_000)]
    public async Task EnqueueAsync_Around50000MessagesPer30Seconds_ShouldProcessAllMessages()
    {
        var mockConnectionEventRepository = new Mock<IConnectionEventRepository>();
        mockConnectionEventRepository.Setup(r => r.StoreAsync(It.IsAny<long>(), 
                It.IsAny<string>(), 
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<MessageProcessorService>();
                services.AddHostedService(sp => sp.GetRequiredService<MessageProcessorService>());
                
                services.AddScoped<IConnectionEventRepository>(_ => mockConnectionEventRepository.Object);
            })
            .Build();

        await host.StartAsync();

        var messageProcessorService = host.Services.GetRequiredService<MessageProcessorService>();
        
        var scenario = Scenario.Create("message-processor-service-load", async context =>
            {
                await messageProcessorService.EnqueueAsync($"{context.InvocationNumber}, 127.0.0.1");
                return Response.Ok();
            })
            .WithLoadSimulations(Simulation.InjectRandom(minRate:2000, maxRate:5000, interval: TimeSpan.FromSeconds(1), during: TimeSpan.FromSeconds(30)));

        var result = NBomberRunner
            .RegisterScenarios(scenario)
            .Run();
        
        Assert.True(result.AllOkCount >= 50_000, $"Only processed {result.AllOkCount} messages, expected >= 50000");
        Assert.True(result.AllFailCount == 0, $"Failed to process messages: {result.AllFailCount}");
        
        await host.StopAsync();
    }
}
