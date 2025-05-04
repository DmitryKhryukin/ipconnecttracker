using IpConnectTracker.WriterService.DataAccess.Abstractions.Repository;
using IpConnectTracker.WriterService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace IpConnectTracker.WriterService.UnitTests.Services;

public class MessageProcessorServiceTests
{

    [Fact]
    public async Task EnqueueAsync_SomeMessagesAreInvalid_ShouldProcessOnlyValidMessages()
    {
        var mockRepo = new Mock<IConnectionEventRepository>();
        var mockLogger = new Mock<ILogger<MessageProcessorService>>();

        var serviceProvider = new Mock<IServiceProvider>();
        var serviceScope = new Mock<IServiceScope>();
        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        
        serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
        serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);
        serviceProvider.Setup(x => x.GetService(typeof(IConnectionEventRepository))).Returns(mockRepo.Object);
        
        var service = new MessageProcessorService(mockLogger.Object, serviceScopeFactory.Object);


        var cancellationToken = new CancellationTokenSource();
        var backgroundTask = service.StartAsync(cancellationToken.Token);

        long userId = 1001;
        var validIpAddress = "127.0.0.1";
        var validMessage = $"{userId},{validIpAddress}";
        var invalidMessage = "invalid_format";
        
        await service.EnqueueAsync(validMessage); 
        await service.EnqueueAsync(validMessage); 
        await service.EnqueueAsync(validMessage); 
        
        await service.EnqueueAsync(invalidMessage); 
        await service.EnqueueAsync(invalidMessage); 

        await cancellationToken.CancelAsync();
        await backgroundTask;
        
        mockRepo.Verify(
            x => x.UpsertAsync(
                It.Is<long>(x => x == userId),
                It.Is<string>(x => x == validIpAddress),
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid message format")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Exactly(2));
    }
}