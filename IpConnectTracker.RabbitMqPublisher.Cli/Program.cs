using System.Text;
using IpConnectTracker.Cli.Config;
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

            Console.WriteLine($"Publishing {options.Count} messages to queue '{options.Queue}'...");

            var rand = new Random();
            for (int i = 0; i < options.Count; i++)
            {
                var userId = options.Random ? rand.Next(1, 200000) : i;
                
                var defaultIpAddress = "127.0.0.1";
                var ipAddress =  options.Random ? GetRandomIpAddress(options, rand, i) : defaultIpAddress;

                var message = $"{userId},{ipAddress}";
                var messageBody = Encoding.UTF8.GetBytes(message);

                await channel.BasicPublishAsync(exchange: "", routingKey: options.Queue, body: messageBody);

                if (options.Verbose)
                {
                    Console.WriteLine($"Sent: {message}");
                }
            }

            Console.WriteLine("Done.");
        }

        private static string GetRandomIpAddress(CliOptions options, Random rand, int i)
        {
            return$"{rand.Next(1, 255)}.{rand.Next(0, 255)}.{rand.Next(0, 255)}.{rand.Next(1, 255)}";
        }
    }
}
