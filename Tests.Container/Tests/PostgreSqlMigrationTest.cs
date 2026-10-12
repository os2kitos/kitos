using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Tests.Container.Helpers;

namespace Tests.Container.Tests;

[Collection(MigrationTestsCollection.Name)]
public sealed class PostgreSqlMigrationTest : IAsyncLifetime
{
    private const string DatabaseName = "kitos";
    private const string HangfireDatabaseName = "hangfire";
    private const string UserName = "postgres";
    private const string Password = "postgres";

    private readonly IContainer _container = new ContainerBuilder("postgres:16-alpine")
        .WithEnvironment("POSTGRES_DB", DatabaseName)
        .WithEnvironment("POSTGRES_USER", UserName)
        .WithEnvironment("POSTGRES_PASSWORD", Password)
        .WithPortBinding(5432, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
        .Build();

    public Task InitializeAsync() => _container.StartAsync();

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    [Fact]
    public async Task Can_Run_Migrations_Against_Postgresql()
    {
        var connectionString =
            $"Host={_container.Hostname};Port={_container.GetMappedPublicPort(5432)};Database={DatabaseName};Username={UserName};Password={Password}";
        var hangfireConnectionString =
            $"Host={_container.Hostname};Port={_container.GetMappedPublicPort(5432)};Database={HangfireDatabaseName};Username={UserName};Password={Password}";

        await MigrationTestHelper.AssertNoPendingMigrationsAsync("PostgreSql", connectionString, hangfireConnectionString);
    }
}
