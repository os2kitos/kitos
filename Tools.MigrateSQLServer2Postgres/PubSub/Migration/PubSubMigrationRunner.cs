using System.Data;
using Microsoft.Data.SqlClient;
using Npgsql;
using Spectre.Console;
using Tools.MigrateSQLServer2Postgres.Cli;
using Tools.MigrateSQLServer2Postgres.Migration;

namespace Tools.MigrateSQLServer2Postgres.PubSub.Migration;

internal sealed class PubSubMigrationRunner
{
    private static readonly TableRef SourceTable = new("dbo", "Subscriptions");
    private static readonly TableRef TargetTable = new("public", "Subscriptions");

    public async Task<int> RunAsync(CommandLineOptions options, bool isInteractive = false)
    {
        await using var sourceConnection = new SqlConnection(options.SourceConnectionString);
        await sourceConnection.OpenAsync();
        CliConsole.Success("SQL Server source connection OK.");

        var effectiveOptions = options;
        var freshTargetPrepared = false;
        NpgsqlConnection? targetConnection = null;

        try
        {
            try
            {
                targetConnection = await OpenTargetConnectionAsync(options.TargetConnectionString);
                CliConsole.Success("PostgreSQL target connection OK.");
            }
            catch (PostgresException exception) when (IsMissingDatabase(exception))
            {
                CliConsole.Warning("Target PostgreSQL database does not exist.");
                var shouldCreate = !isInteractive || AnsiConsole.Confirm("Create a new empty target database with the required structure now?", true);
                if (!shouldCreate)
                {
                    throw new CommandLineException("Target PostgreSQL database does not exist.");
                }

                var prepareResult = await RunPrepareTargetAsync(options.TargetConnectionString, requireConfirmation: false);
                if (prepareResult != 0)
                {
                    throw new CommandLineException("Failed to create the target PostgreSQL database.");
                }

                effectiveOptions = options with { AllowNonEmptyTarget = false };
                freshTargetPrepared = true;
                targetConnection = await OpenTargetConnectionAsync(options.TargetConnectionString);
                CliConsole.Success("PostgreSQL target connection OK.");
            }

            if (targetConnection is null)
            {
                throw new CommandLineException("Target PostgreSQL connection could not be established.");
            }

            if (!freshTargetPrepared)
            {
                var targetTableExists = await PostgresTableExistsAsync(targetConnection, TargetTable);

                if (!targetTableExists)
                {
                    // Database exists but schema is not bootstrapped yet (e.g. freshly created empty DB).
                    CliConsole.Warning("Target PostgreSQL database exists but the Subscriptions table is missing.");
                    var shouldBootstrap = !isInteractive || AnsiConsole.Confirm("Bootstrap the target database with EF Core migrations now?", true);
                    if (!shouldBootstrap)
                    {
                        throw new CommandLineException("Target database is not bootstrapped. Run with --interactive or prepare the schema first.");
                    }

                    var bootstrapper = new PubSubTargetBootstrapper();
                    await bootstrapper.BootstrapFreshDatabaseAsync(options.TargetConnectionString);
                    freshTargetPrepared = true;
                    effectiveOptions = options with { AllowNonEmptyTarget = false };
                }
                else if (isInteractive && !effectiveOptions.AllowNonEmptyTarget)
                {
                    var hasData = await TableHasDataAsync(targetConnection, TargetTable);
                    if (hasData)
                    {
                        CliConsole.Warning($"Target table {TargetTable} is not empty.");
                        var recreate = AnsiConsole.Confirm("Recreate the target database as a new empty database with the required structure?", true);
                        if (recreate)
                        {
                            await targetConnection.CloseAsync();
                            await targetConnection.DisposeAsync();
                            NpgsqlConnection.ClearAllPools();

                            var prepareResult = await RunPrepareTargetAsync(effectiveOptions.TargetConnectionString, requireConfirmation: false);
                            if (prepareResult != 0)
                            {
                                throw new CommandLineException("Failed to recreate the target PostgreSQL database.");
                            }

                            targetConnection = await OpenTargetConnectionAsync(options.TargetConnectionString);
                            effectiveOptions = effectiveOptions with { AllowNonEmptyTarget = false };
                            freshTargetPrepared = true;
                        }
                    }
                }
            }

            var readinessResult = await RunReadinessChecksAsync(sourceConnection, targetConnection, effectiveOptions);
            if (readinessResult != 0)
            {
                return readinessResult;
            }

            var executeResult = await RunExecuteAsync(sourceConnection, targetConnection, effectiveOptions);
            if (executeResult != 0)
            {
                return executeResult;
            }

            return await RunValidateAsync(sourceConnection, targetConnection);
        }
        finally
        {
            if (targetConnection is not null)
            {
                await targetConnection.DisposeAsync();
            }
        }
    }

    private static async Task<int> RunReadinessChecksAsync(SqlConnection sourceConnection, NpgsqlConnection targetConnection, CommandLineOptions options)
    {
        CliConsole.Info("Running readiness checks...");

        var sourceTableExists = await SqlServerTableExistsAsync(sourceConnection, SourceTable);
        var targetTableExists = await PostgresTableExistsAsync(targetConnection, TargetTable);
        var hasData = targetTableExists && await TableHasDataAsync(targetConnection, TargetTable);

        var summary = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Check")
            .AddColumn("Result");

        summary.AddRow("Source table (SQL Server)", sourceTableExists ? $"[green]{Markup.Escape(SourceTable.ToString())}[/]" : $"[red]Not found[/]");
        summary.AddRow("Target table (PostgreSQL)", targetTableExists ? $"[green]{Markup.Escape(TargetTable.ToString())}[/]" : $"[red]Not found[/]");
        summary.AddRow("Target empty", !hasData ? "[green]Yes[/]" : "[yellow]No[/]");
        AnsiConsole.Write(summary);

        if (!sourceTableExists)
        {
            CliConsole.Error($"Readiness check failed: source table {SourceTable} not found in SQL Server.");
            return -1;
        }

        if (!targetTableExists)
        {
            CliConsole.Error($"Readiness check failed: target table {TargetTable} not found in PostgreSQL. Run with --interactive or prepare the target database first.");
            return -2;
        }

        if (!options.AllowNonEmptyTarget && hasData)
        {
            CliConsole.Error($"Readiness check failed: target table {TargetTable} is not empty. Use --allow-non-empty-target to override.");
            return -3;
        }

        CliConsole.Success("Readiness checks completed successfully.");
        return 0;
    }

    private static async Task<int> RunPrepareTargetAsync(string targetConnectionString, bool requireConfirmation)
    {
        CliConsole.Info("Preparing fresh PostgreSQL target...");

        var targetBuilder = new NpgsqlConnectionStringBuilder(targetConnectionString);
        var targetDatabase = targetBuilder.Database;

        if (requireConfirmation)
        {
            var overwriteConfirmed = AnsiConsole.Confirm($"Overwrite PostgreSQL database '{targetDatabase}'? This will delete all existing data.", false);
            if (!overwriteConfirmed)
            {
                CliConsole.Warning("Target preparation cancelled.");
                return -1;
            }
        }

        await PubSubTargetBootstrapper.DropAndRecreateDatabaseAsync(targetConnectionString);

        var bootstrapper = new PubSubTargetBootstrapper();
        await bootstrapper.BootstrapFreshDatabaseAsync(targetConnectionString);

        CliConsole.Success("Target schema is ready.");
        return 0;
    }

    private static async Task<int> RunExecuteAsync(SqlConnection sourceConnection, NpgsqlConnection targetConnection, CommandLineOptions options)
    {
        CliConsole.Info("Copying data...");

        await MigrationJournal.EnsureJournalTableAsync(targetConnection);

        var alreadySucceeded = await MigrationJournal.WasSuccessfulAsync(targetConnection, TargetTable);
        if (alreadySucceeded && !options.AllowNonEmptyTarget)
        {
            CliConsole.Info($"Table {TargetTable} was already successfully migrated (journal). Skipping.");
            return 0;
        }

        try
        {
            await MigrationJournal.MarkStartedAsync(targetConnection, TargetTable);

            var sourceRowCount = await CountRowsSqlServerAsync(sourceConnection, SourceTable);
            var rowsCopied = await CopySubscriptionsAsync(sourceConnection, targetConnection);

            await MigrationJournal.MarkSucceededAsync(targetConnection, TargetTable, rowsCopied);

            CliConsole.RenderExecutionSummary([new TableExecutionResult(TargetTable, TableExecutionStatus.Succeeded, sourceRowCount, rowsCopied)]);
            CliConsole.Success("Migration completed successfully.");
            return 0;
        }
        catch (Exception exception)
        {
            await MigrationJournal.MarkFailedAsync(targetConnection, TargetTable, exception.Message);
            CliConsole.Exception($"Failed to migrate {SourceTable} -> {TargetTable}.", exception);
            return -1;
        }
    }

    private static async Task<long> CopySubscriptionsAsync(SqlConnection sourceConnection, NpgsqlConnection targetConnection)
    {
        var sourceColumns = await GetSqlServerColumnsAsync(sourceConnection, SourceTable);
        if (sourceColumns.Count == 0)
        {
            throw new InvalidOperationException($"No columns found for source table {SourceTable}.");
        }

        var selectSql = $"SELECT {string.Join(", ", sourceColumns.Select(c => $"[{c}]"))} FROM [{SourceTable.Schema}].[{SourceTable.Name}];";
        var insertColumns = string.Join(", ", sourceColumns.Select(SchemaDiscovery.QuotePostgresIdentifier));
        var paramList = string.Join(", ", Enumerable.Range(0, sourceColumns.Count).Select(i => $"@p{i}"));
        var insertSql = $"INSERT INTO {SchemaDiscovery.QualifyPostgresTable(TargetTable)} ({insertColumns}) VALUES ({paramList});";

        await using var transaction = await targetConnection.BeginTransactionAsync();

        long copiedRows;
        {
            await using var selectCommand = new SqlCommand(selectSql, sourceConnection);
            await using var reader = await selectCommand.ExecuteReaderAsync();
            await using var insertCommand = new NpgsqlCommand(insertSql, targetConnection, transaction);

            for (var i = 0; i < sourceColumns.Count; i++)
            {
                insertCommand.Parameters.Add(new NpgsqlParameter($"@p{i}", DBNull.Value));
            }

            copiedRows = 0;
            while (await reader.ReadAsync())
            {
                for (var i = 0; i < sourceColumns.Count; i++)
                {
                    insertCommand.Parameters[i].Value = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                }

                await insertCommand.ExecuteNonQueryAsync();
                copiedRows++;
            }
        }

        await transaction.CommitAsync();
        return copiedRows;
    }

    private static async Task<int> RunValidateAsync(SqlConnection sourceConnection, NpgsqlConnection targetConnection)
    {
        CliConsole.Info("Validating row counts...");

        var sourceCount = await CountRowsSqlServerAsync(sourceConnection, SourceTable);
        var targetCount = await CountRowsPostgresAsync(targetConnection, TargetTable);

        var summary = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Metric")
            .AddColumn("Value");

        summary.AddRow("Compared tables", "1");
        summary.AddRow("Mismatches", sourceCount == targetCount ? "[green]0[/]" : "[red]1[/]");
        AnsiConsole.Write(summary);

        if (sourceCount != targetCount)
        {
            var mismatchTable = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("Table")
                .AddColumn("Source rows")
                .AddColumn("Target rows");

            mismatchTable.AddRow(
                Markup.Escape(SourceTable.ToString()),
                sourceCount.ToString(),
                targetCount.ToString());

            AnsiConsole.Write(mismatchTable);
            CliConsole.Error("Validation failed: row count mismatch.");
            return -1;
        }

        CliConsole.Success($"Validation passed. {sourceCount} row(s) verified.");
        return 0;
    }

    private static async Task<bool> SqlServerTableExistsAsync(SqlConnection connection, TableRef table)
    {
        const string sql = @"
SELECT COUNT(1)
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = @schemaName AND t.name = @tableName;";
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@schemaName", table.Schema);
        command.Parameters.AddWithValue("@tableName", table.Name);
        var value = await command.ExecuteScalarAsync();
        return Convert.ToInt32(value) > 0;
    }

    private static async Task<bool> PostgresTableExistsAsync(NpgsqlConnection connection, TableRef table)
    {
        const string sql = @"
SELECT COUNT(1)
FROM information_schema.tables
WHERE table_schema = @schemaName AND table_name = @tableName AND table_type = 'BASE TABLE';";
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@schemaName", table.Schema);
        command.Parameters.AddWithValue("@tableName", table.Name);
        var value = await command.ExecuteScalarAsync();
        return Convert.ToInt64(value) > 0;
    }

    private static async Task<bool> TableHasDataAsync(NpgsqlConnection connection, TableRef table)
    {
        var sql = $"SELECT EXISTS (SELECT 1 FROM {SchemaDiscovery.QualifyPostgresTable(table)} LIMIT 1);";
        await using var command = new NpgsqlCommand(sql, connection);
        var exists = await command.ExecuteScalarAsync();
        return exists is bool b && b;
    }

    private static async Task<List<string>> GetSqlServerColumnsAsync(SqlConnection connection, TableRef table)
    {
        const string sql = @"
SELECT c.name
FROM sys.columns c
INNER JOIN sys.tables t ON t.object_id = c.object_id
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = @schemaName AND t.name = @tableName
ORDER BY c.column_id;";

        var result = new List<string>();
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@schemaName", table.Schema);
        command.Parameters.AddWithValue("@tableName", table.Name);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }

    private static async Task<long> CountRowsSqlServerAsync(SqlConnection connection, TableRef table)
    {
        var sql = $"SELECT COUNT_BIG(1) FROM [{table.Schema}].[{table.Name}];";
        await using var command = new SqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return Convert.ToInt64(value);
    }

    private static async Task<long> CountRowsPostgresAsync(NpgsqlConnection connection, TableRef table)
    {
        var sql = $"SELECT COUNT(*) FROM {SchemaDiscovery.QualifyPostgresTable(table)};";
        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return Convert.ToInt64(value);
    }

    private static async Task<NpgsqlConnection> OpenTargetConnectionAsync(string targetConnectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(targetConnectionString)
        {
            IncludeErrorDetail = true
        };
        var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static bool IsMissingDatabase(PostgresException exception)
    {
        return string.Equals(exception.SqlState, PostgresErrorCodes.InvalidCatalogName, StringComparison.Ordinal);
    }
}
