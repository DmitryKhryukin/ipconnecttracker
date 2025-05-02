namespace IpConnectTracker.WriterService.DataAccess.Abstractions.Repository;

public interface IConnectionEventRepository
{
    Task StoreAsync(long userId, string ipAddress, DateTime timestamp, CancellationToken cancellationToken);
}