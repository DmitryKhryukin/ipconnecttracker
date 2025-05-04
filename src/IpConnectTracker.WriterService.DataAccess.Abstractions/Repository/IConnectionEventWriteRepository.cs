namespace IpConnectTracker.WriterService.DataAccess.Abstractions.Repository;

public interface IConnectionEventWriteRepository
{
    Task UpsertAsync(long userId, string ipAddress, DateTime timestamp, CancellationToken cancellationToken);
}