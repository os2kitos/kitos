using Microsoft.EntityFrameworkCore;
using Npgsql;
using PubSub.Infrastructure.DataAccess;
using Tools.MigrateSQLServer2Postgres.Cli;

namespace Tools.MigrateSQLServer2Postgres.PubSub.Migration;

internal sealed class PubSubTargetBootstrapper
{
    public async Task BootstrapFreshDatabaseAsync(string targetConnectionString)
    {
        CliConsole.Info("Applying PubSub EF Core migrations to target database.");

        var optionsBuilder = new DbContextOptionsBuilder<PubSubContext>();
        optionsBuilder.UseNpgsql(targetConnectionString);

        await using var context = new PubSubContext(optionsBuilder.Options);
        await context.Database.MigrateAsync();

        CliConsole.Success("PubSub target schema is ready.");
    }

    public static async Task CreateDatabaseIfNotExistsAsync(string targetConnectionString)
    {
        var targetBuilder = new NpgsqlConnectionStringBuilder(targetConnectionString);
        if (string.IsNullOrWhiteSpace(targetBuilder.Database))
        {
            throw new CommandLineException("Target connection string must include a database name.");
        }

        var targetDatabase = targetBuilder.Database;
        var adminBuilder = new NpgsqlConnectionStringBuilder(targetBuilder.ConnectionString)
        {
            Database = "postgres",
            Pooling = false
        };

        await using var adminConnection = new NpgsqlConnection(adminBuilder.ConnectionString);
        await adminConnection.OpenAsync();

        var exists = await DatabaseExistsAsync(adminConnection, targetDatabase);
        if (!exists)
        {
            var createSql = $"CREATE DATABASE {QuoteIdentifier(targetDatabase)};";
            await using var createCommand = new NpgsqlCommand(createSql, adminConnection);
            await createCommand.ExecuteNonQueryAsync();
            CliConsole.Success($"Created target database '{targetDatabase}'.");
        }
    }

    public static async Task DropAndRecreateDatabaseAsync(string targetConnectionString)
    {
        var targetBuilder = new NpgsqlConnectionStringBuilder(targetConnectionString);
        if (string.IsNullOrWhiteSpace(targetBuilder.Database))
        {
            throw new CommandLineException("Target connection string must include a database name.");
        }

        var targetDatabase = targetBuilder.Database;
        var adminBuilder = new NpgsqlConnectionStringBuilder(targetBuilder.ConnectionString)
        {
            Database = "postgres",
            Pooling = false
        };

        await using var adminConnection = new NpgsqlConnection(adminBuilder.ConnectionString);
        await adminConnection.OpenAsync();

        if (await DatabaseExistsAsync(adminConnection, targetDatabase))
        {
            await TerminateConnectionsAsync(adminConnection, targetDatabase);
            NpgsqlConnection.ClearAllPools();
            var dropSql = $"DROP DATABASE {QuoteIdentifier(targetDatabase)};";
            await using var dropCommand = new NpgsqlCommand(dropSql, adminConnection);
            await dropCommand.ExecuteNonQueryAsync();
            CliConsole.Info($"Dropped target database '{targetDatabase}'.");
        }

        var createSql = $"CREATE DATABASE {QuoteIdentifier(targetDatabase)};";
        await using var createCommand = new NpgsqlCommand(createSql, adminConnection);
        await createCommand.ExecuteNonQueryAsync();
        CliConsole.Success($"Created target database '{targetDatabase}'.");

        NpgsqlConnection.ClearAllPools();
    }

    private static async Task<bool> DatabaseExistsAsync(NpgsqlConnection adminConnection, string databaseName)
    {
        const string sql = "SELECT EXISTS (SELECT 1 FROM pg_database WHERE datname = @databaseName);";
        await using var command = new NpgsqlCommand(sql, adminConnection);
        command.Parameters.AddWithValue("@databaseName", databaseName);
        var value = await command.ExecuteScalarAsync();
        return value is true;
    }

    private static async Task TerminateConnectionsAsync(NpgsqlConnection adminConnection, string databaseName)
    {
        const string sql = @"
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE datname = @databaseName AND pid <> pg_backend_pid();";
        await using var command = new NpgsqlCommand(sql, adminConnection);
        command.Parameters.AddWithValue("@databaseName", databaseName);
        await command.ExecuteNonQueryAsync();
    }

    private static string QuoteIdentifier(string identifier)
    {
        return $"\"{identifier.Replace("\"", "\"\"")}\"";
    }
}
