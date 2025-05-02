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

    Task<DateTime?> GetLastConnectionByIpAsync(string ip, CancellationToken cancellationToken = default);
}