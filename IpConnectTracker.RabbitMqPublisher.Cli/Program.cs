using System.Text;
using IpConnectTracker.Cli.Helpers;
using RabbitMQ.Client;

namespace IpConnectTracker.Cli
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var options = CliArgumentsParser.ParseArgs(args);

            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: options.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            Console.WriteLine($"Publishing {options.Count} messages from {options.UserCount} users to queue '{options.Queue}'...");

            var userCount = options.UserCount;
            var rand = new Random();
            
            var userIps = new Dictionary<long, List<string>>();
            var sharedIps = GenerateSharedIps(rand, options.UserCount / 10);
            
            for (int i = 0; i < options.Count; i++)
            {
                long userId = rand.Next(10_000, options.UserCount + 10_000);
                
                var ipAddress = string.Empty;
                
                if (rand.NextDouble() < 0.3 || // 30% chance to use a new IP for this user
                    !userIps.ContainsKey(userId) ||  
                    userIps[userId].Count == 0)
                {
                    // 20% chance to use a shared IP
                    if (rand.NextDouble() < 0.1 && // 10% chance to use a shared IP for this user
                        sharedIps.Count > 0)
                    {
                        ipAddress = sharedIps[rand.Next(sharedIps.Count)];
                    }
                    else
                    {
                        ipAddress = GetClusteredIp(rand);
                    }
                    
                    if (!userIps.ContainsKey(userId))
                    {
                        userIps[userId] = new List<string>();
                    }
                    
                    userIps[userId].Add(ipAddress);
                }
                else
                {
                    // use one of the user's existing ips
                    var userIpList = userIps[userId];
                    ipAddress = userIpList[rand.Next(userIpList.Count)];
                }

                var message = $"{userId},{ipAddress}";
                var messageBody = Encoding.UTF8.GetBytes(message);

                await channel.BasicPublishAsync(exchange: "", routingKey: options.Queue, body: messageBody);
                
                Console.WriteLine($"Sent: {message}");
            }

            Console.WriteLine("Publisher statistics:");
            Console.WriteLine($"Unique users: {userIps.Count}");
            Console.WriteLine($"Average IPs per user: {userIps.Average(x => x.Value.Count)}");
        }

        private static List<string> GenerateSharedIps(Random rand, int count)
        {
            var sharedIps = new List<string>();
            for (int i = 0; i < count; i++)
            {
                sharedIps.Add(GetClusteredIp(rand));
            }
            return sharedIps;
        }
        
        private static string GetClusteredIp(Random rand)
        {
            var subnet = rand.Next(1, 5);
            return $"{subnet}.{rand.Next(0, 255)}.{rand.Next(0, 255)}.{rand.Next(1, 255)}";
        }
    }
}
