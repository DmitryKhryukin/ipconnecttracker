namespace IpConnectTracker.ReaderService.Api.Model;

public record UserConnectionDto(long UserId, string Ip, DateTime Timestamp);