namespace IpConnectTracker.ReaderService.DataAccess.Abstractions.Repository;

public interface IConnectionEventReadRepository
{
    Task<IEnumerable<long>> GetUsersByIpPrefixAsync(
        string ipPrefix,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<string>> GetUserIpsAsync(long userId, CancellationToken cancellationToken = default);
    
    Task<(string ip, DateTime timestamp)?> GetUserLastConnectionAsync(long userId, CancellationToken cancellationToken = default);

    Task<IEnumerable<(long userId, string ip, DateTime timestamp)>> GetUsersLastConnectionsAsync(
        long[] userIds,
        CancellationToken cancellationToken = default);
    
    Task<DateTime?> GetLastConnectionByUserAndIpAsync(long userId, string ip, CancellationToken cancellationToken = default);
}