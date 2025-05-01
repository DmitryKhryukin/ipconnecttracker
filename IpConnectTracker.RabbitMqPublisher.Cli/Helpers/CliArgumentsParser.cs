using IpConnectTracker.Cli.Config;

namespace IpConnectTracker.Cli.Helpers;

public static class CliArgumentsParser
{
    private const string CountArg = "--count";
    private const string QueueArg = "--queue";
    private const string RandomArg = "--random";
    private const string VerboseArg = "--verbose";
    
    private const int DefaultCount = 1000;
    private const string DefaultQueue = "ip_connects";
    
    //TODO: tests
    public static CliOptions ParseArgs(string[] args)
    {
        var options = new CliOptions();
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

                case RandomArg:
                    options.Random = true;
                    i++;
                    break;

                case VerboseArg:
                    options.Verbose = true;
                    i++;
                    break;

                default:
                    i++;
                    break;
            }
        }

        return options;
    }
}