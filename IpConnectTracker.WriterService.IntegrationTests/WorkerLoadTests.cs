using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBomber.CSharp;

namespace IpConnectTracker.WriterService.IntegrationTests;

public class WorkerLoadTests
{
    [Fact(Timeout = 60_000)]
    public async Task EnqueueAsync_Around50000MessagesPer30Seconds_ShouldProcessAllMessages()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<Worker>();
                services.AddHostedService(sp => sp.GetRequiredService<Worker>());
            })
            .Build();

        await host.StartAsync();

        var worker = host.Services.GetRequiredService<Worker>();
        
        var scenario = Scenario.Create("worker-load", async context =>
            {
                await worker.EnqueueAsync($"message-{context.InvocationNumber}");
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
