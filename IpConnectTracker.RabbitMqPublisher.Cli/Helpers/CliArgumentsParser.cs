using IpConnectTracker.Cli.Config;

namespace IpConnectTracker.Cli.Helpers;

public static class CliArgumentsParser
{
    private const string CountArg = "--count";
    private const string QueueArg = "--queue";
    private const string UserCountArg = "--user-count";

    public static int DefaultCount { get; } = 3_000_000;
    public static int DefaultUserCount { get; } = 1000;
    public static string DefaultQueue { get; } = "ip_connects";
    
    public static CliOptions ParseArgs(string[] args)
    {
        var options = new CliOptions()
        {
            Count = DefaultCount,
            Queue = DefaultQueue,
            UserCount = DefaultUserCount
        };
        
        int i = 0;

        while (i < args.Length)
        {
            var arg = args[i];

            switch (arg)
            {
                case CountArg:
                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out var count))
                    {
                        options.Count = count;
                        i += 2;
                    }
                    else
                    {
                        options.Count = DefaultCount;
                        i++;
                    }
                    break;

                case QueueArg:
                    if (i + 1 < args.Length)
                    {
                        options.Queue = args[i + 1];
                        i += 2;
                    }
                    else
                    {
                        options.Queue = DefaultQueue;
                        i++;
                    }
                    break;

                case UserCountArg:
                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out var userCount))
                    {
                        options.UserCount = userCount;
                        i += 2;
                    }
                    else
                    {
                        options.UserCount = DefaultUserCount;
                        i++;
                    }
                    break;

                default:
                    i++;
                    break;
            }
        }

        return options;
    }
}