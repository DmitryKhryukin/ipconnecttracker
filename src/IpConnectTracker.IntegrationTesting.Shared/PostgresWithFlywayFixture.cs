using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;
using Xunit;

namespace IpConnectTracker.IntegrationTesting.Shared;

public class PostgresWithFlywayFixture : IAsyncLifetime
{
    private const string Username = "app";
    private const string Password = "secret";
    private const string DbName = "iptracker";

    public INetwork Network { get; }
    public IContainer PostgresContainer { get; }
    public IContainer FlywayContainer { get; }

    public PostgresWithFlywayFixture()
    {
        Network = new NetworkBuilder()
            .WithName("postgres-test-network")
            .Build();

        PostgresContainer = new ContainerBuilder()
            .WithImage("postgres:17.4-alpine") // version should be the same as docker-compose.yaml
            .WithEnvironment("POSTGRES_DB", "iptracker")
            .WithEnvironment("POSTGRES_USER", "app")
            .WithEnvironment("POSTGRES_PASSWORD", "secret")
            .WithPortBinding(5432, true)
            .WithNetwork(Network)
            .WithNetworkAliases("postgres-host")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilCommandIsCompleted("pg_isready -U app -d iptracker"))
            .WithCleanUp(true)
            .WithName("testcontainer-postgres")
            .Build();
        
        var migrationsFolderPath = GetMigrationsFolderPath();
        
        FlywayContainer = new ContainerBuilder()
            .WithImage("flyway/flyway:11.8") // version should be the same as docker-compose.yaml
            .WithBindMount(migrationsFolderPath, "/flyway/sql")
            .WithNetwork(Network)
            .WithCommand(
                "-url=jdbc:postgresql://postgres-host:5432/iptracker",
                "-user=app",
                "-password=secret",
                "migrate")
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("Successfully applied"))
            .WithCleanUp(true)
            .WithName("testcontainer-flyway")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await Network.CreateAsync();
        await PostgresContainer.StartAsync();
        await FlywayContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await FlywayContainer.DisposeAsync();
        await PostgresContainer.DisposeAsync();
        await Network.DeleteAsync();
    }

    private string GetMigrationsFolderPath()
    {
        var testProjectDir = Directory.GetCurrentDirectory();
        
        var solutionDir = Directory.GetParent(testProjectDir)?.Parent?.Parent?.Parent?.FullName 
                          ?? throw new InvalidOperationException("Could not find solution directory");

        var result = Path.Combine(
            solutionDir,
            "IpConnectTracker.WriterService.DataAccess.PostgreSQL",
            "Migrations");
        
        if (!Directory.Exists(result))
        {
            throw new DirectoryNotFoundException($"Migrations directory not found at: {result}");
        }

        return result;
    }
}