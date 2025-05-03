namespace IpConnectTracker.WriterService.Helpers;

public static class MessageParser
{
    public static bool TryParse(string message, out long userId, out string ip)
    {
        userId = 0;
        ip = string.Empty;
        
        if (string.IsNullOrWhiteSpace(message))
            return false;
        
        var parts = message.Split(',');
        
        var shouldHaveTwoParts = parts.Length == 2;
        var firstPartIsLong = long.TryParse(parts[0], out userId);
        
        if (!shouldHaveTwoParts || !firstPartIsLong)
            return false;

        ip = parts[1];
        
        var secondPartIsNotNullOrEmpty = !string.IsNullOrWhiteSpace(parts[1]);
        
        if (!secondPartIsNotNullOrEmpty)
            return false;
        
        return true;
    }
}