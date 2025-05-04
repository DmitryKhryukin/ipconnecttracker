using System.Text.RegularExpressions;

namespace IpConnectTracker.ReaderService.Api.Validation;

public static class IpAddressPrefixValidator
{
    // "192", "192.168", "10.0.0." etc
    private static readonly Regex Ipv4PrefixRegex = new(@"^(\d{1,3}\.){0,3}\d{0,3}$");

    // "2001", "2001:db8", "fe80::", "2001:db8:abcd:" etc
    private static readonly Regex Ipv6PrefixRegex = new(@"^([0-9a-fA-F]{0,4}(:|::)){0,7}[0-9a-fA-F]{0,4}$");

    public static bool IsValid(string ipAddressPrefix)
    {
        var result = false;

        if (!string.IsNullOrWhiteSpace(ipAddressPrefix))
        {
            result = Ipv4PrefixRegex.IsMatch(ipAddressPrefix) || 
                     Ipv6PrefixRegex.IsMatch(ipAddressPrefix);
        }

        return result;
    }
}