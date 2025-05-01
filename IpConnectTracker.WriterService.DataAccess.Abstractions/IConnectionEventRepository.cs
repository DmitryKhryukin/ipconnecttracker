namespace IpConnectTracker.WriterService.DataAccess.Abstractions;

public interface IConnectionEventRepository
{
    Task StoreAsync(long userId, string ipAddress, DateTime timestamp, CancellationToken cancellationToken);
}