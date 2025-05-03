namespace IpConnectTracker.WriterService.DataAccess.Abstractions.Repository;

public interface IConnectionEventRepository
{
    Task UpsertAsync(long userId, string ipAddress, DateTime timestamp, CancellationToken cancellationToken);
}