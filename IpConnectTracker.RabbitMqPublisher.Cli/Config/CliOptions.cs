namespace IpConnectTracker.Cli.Config;

public class CliOptions
{
    public int Count { get; set; } = 1000;
    public string Queue { get; set; } = "ip_connects";
    public bool Random { get; set; }
    public bool Verbose { get; set; }
}