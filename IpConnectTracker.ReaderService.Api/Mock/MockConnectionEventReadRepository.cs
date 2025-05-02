using IpConnectTracker.ReaderService.DataAccess.Abstractions;

namespace IpConnectTracker.ReaderService.Api;

public class MockConnectionEventReadRepository : IConnectionEventReadRepository
{
    public Task<IEnumerable<long>> GetUsersByIpPrefixAsync(string ipPrefix, int skip = 0, int take = 100,
        CancellationToken cancellationToken = default)
    {
        var userIds = new List<long> { 1234567, 9876543 };
        return Task.FromResult<IEnumerable<long>>(userIds);
    }

    public Task<IEnumerable<string>> GetUserIpsAsync(long userId, CancellationToken cancellationToken = default)
    {
        var ips = new List<string>
        {
            "31.214.157.141",
            "62.4.36.194"
        };

        return Task.FromResult<IEnumerable<string>>(ips);
    }

    public Task<(string ip, DateTime timestamp)?> GetUserLastConnectionAsync(long userId,
        CancellationToken cancellationToken = default)
    {
        var result = ("192.168.1.100", DateTime.UtcNow.AddMinutes(-5));
        return Task.FromResult<(string, DateTime)?>(result);
    }

    public Task<DateTime?> GetLastConnectionByIpAsync(string ip, CancellationToken cancellationToken = default)
    {
        var timestamp = DateTime.UtcNow.AddMinutes(-10);
        return Task.FromResult<DateTime?>(timestamp);
    }
}