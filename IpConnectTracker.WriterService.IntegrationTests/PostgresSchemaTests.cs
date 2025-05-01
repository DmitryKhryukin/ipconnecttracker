using Dapper;
using Npgsql;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

public class PostgresSchemaTests : IAsyncLifetime
{
    private readonly IContainer _postgresContainer;
    private readonly IContainer _flywayContainer;
    private readonly INetwork _network;

    public PostgresSchemaTests()
    {
        _network = new NetworkBuilder()
            .WithName("postgres-test-network")
            .Build();

        _postgresContainer = new ContainerBuilder()
            .WithImage("postgres:17.4-alpine") // version should be the same as docker-compose.yaml
            .WithEnvironment("POSTGRES_DB", "iptracker")
            .WithEnvironment("POSTGRES_USER", "app")
            .WithEnvironment("POSTGRES_PASSWORD", "secret")
            .WithPortBinding(5432, true)
            .WithNetwork(_network)
            .WithNetworkAliases("postgres-host") // Explicit alias
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilCommandIsCompleted("pg_isready -U app -d iptracker"))
            .WithCleanUp(true)
            .WithName("testcontainer-postgres")
            .Build();
        
        var migrationsFolderPath = GetMigrationsFolderPath();
        
        _flywayContainer = new ContainerBuilder()
            .WithImage("flyway/flyway:11.8") // version should be the same as docker-compose.yaml
            .WithBindMount(migrationsFolderPath, "/flyway/sql")
            .WithNetwork(_network)
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
        await _network.CreateAsync();
        await _postgresContainer.StartAsync();
        await _flywayContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _flywayContainer.DisposeAsync();
        await _postgresContainer.DisposeAsync();
        await _network.DeleteAsync();
    }

    [Fact(Timeout = 60_000)]
    public async Task RunMigrationScripts_Table_user_connection_events_Should_Exist()
    {
        var connectionString = $"Host={_postgresContainer.Hostname};Port={_postgresContainer.GetMappedPublicPort(5432)};Database=iptracker;Username=app;Password=secret";
        
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var result = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'user_connection_events';");

        Assert.Equal(1, result);
    }
    
    //TODO: update this logic to be usable for CI
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