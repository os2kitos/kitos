using System.Data;
using Microsoft.Data.SqlClient;
using Npgsql;
using Spectre.Console;
using Tools.MigrateSQLServer2Postgres.Cli;

namespace Tools.MigrateSQLServer2Postgres.Migration;

internal sealed class MigrationRunner
{
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

                effectiveOptions = options with
                {
                    AllowNonEmptyTarget = false
                };
                freshTargetPrepared = true;

                targetConnection = await OpenTargetConnectionAsync(options.TargetConnectionString);
                CliConsole.Success("PostgreSQL target connection OK.");
            }

            if (targetConnection is null)
            {
                throw new CommandLineException("Target PostgreSQL connection could not be established.");
            }

            if (isInteractive && !effectiveOptions.AllowNonEmptyTarget)
            {
                var adjustedOptions = await RecreateTargetIfUserRequestsItAsync(sourceConnection, targetConnection, effectiveOptions);
                if (!ReferenceEquals(adjustedOptions, effectiveOptions))
                {
                    await targetConnection.CloseAsync();
                    await targetConnection.DisposeAsync();
                    targetConnection = await OpenTargetConnectionAsync(options.TargetConnectionString);
                    effectiveOptions = adjustedOptions;
                    freshTargetPrepared = true;
                }

                if (!freshTargetPrepared)
                {
                    var strategyChoice = AnsiConsole.Prompt(
                        new SelectionPrompt<string>()
                            .Title("Choose [green]migration strategy[/]")
                            .PageSize(10)
                            .AddChoices(
                                "Start fresh (recreate target database)",
                                "Use current settings"));

                    if (strategyChoice == "Start fresh (recreate target database)")
                    {
                        var prepareResult = await RunPrepareTargetAsync(effectiveOptions.TargetConnectionString, requireConfirmation: true);
                        if (prepareResult != 0)
                        {
                            return prepareResult;
                        }

                        await targetConnection.CloseAsync();
                        await targetConnection.DisposeAsync();
                        targetConnection = await OpenTargetConnectionAsync(options.TargetConnectionString);

                        effectiveOptions = effectiveOptions with
                        {
                            AllowNonEmptyTarget = false
                        };
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

        var sourceTables = GetCandidateTables(await SchemaDiscovery.GetSqlServerTablesAsync(sourceConnection));
        var targetTables = GetCandidateTables(await SchemaDiscovery.GetPostgresTablesAsync(targetConnection));
        var matching = SchemaDiscovery.AnalyzeTableMatches(sourceTables, targetTables);
        var compatibility = await AssessTableCompatibilityAsync(sourceConnection, targetConnection, matching.Matches);
        var nonEmptyTable = await FindFirstNonEmptyPostgresTableAsync(targetConnection, targetTables);

        CliConsole.RenderReadinessSummary(
            sourceTables.Count,
            targetTables.Count,
            matching.Matches.Count,
            nonEmptyTable is null,
            nonEmptyTable);

        RenderIssues("Readiness warnings", matching.Warnings.Concat(compatibility.Warnings));
        RenderIssues("Readiness issues", matching.Errors.Concat(compatibility.Errors));

        if (matching.Matches.Count == 0)
        {
            CliConsole.Error("Readiness checks failed: no compatible source/target table matches were found for the migration candidates.");
            return -1;
        }

        if (matching.Errors.Count > 0 || compatibility.Errors.Count > 0)
        {
            CliConsole.Error("Readiness checks failed: source and target schemas are not compatible for a safe migration.");
            return -2;
        }

        if (!options.AllowNonEmptyTarget && nonEmptyTable is not null)
        {
            CliConsole.Error($"Readiness checks failed: target database is not empty. First non-empty migration table: {nonEmptyTable}.");
            return -3;
        }

        CliConsole.Success("Readiness checks completed successfully.");
        return 0;
    }

    private static async Task<int> RunPrepareTargetAsync(string targetConnectionString, bool requireConfirmation)
    {
        CliConsole.Info("Preparing fresh PostgreSQL target...");

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

        await using (var adminConnection = new NpgsqlConnection(adminBuilder.ConnectionString))
        {
            await adminConnection.OpenAsync();

            var databaseExists = await DatabaseExistsAsync(adminConnection, targetDatabase);
            if (databaseExists)
            {
                CliConsole.Warning($"Existing target database '{targetDatabase}' found.");
                if (requireConfirmation)
                {
                    var overwriteConfirmed = AnsiConsole.Confirm($"Overwrite PostgreSQL database '{targetDatabase}'? This will delete all existing data.", false);
                    if (!overwriteConfirmed)
                    {
                        CliConsole.Warning("Target preparation cancelled.");
                        return -1;
                    }
                }

                await DropDatabaseAsync(adminConnection, targetDatabase);
                NpgsqlConnection.ClearAllPools();
                CliConsole.Info($"Dropped target database '{targetDatabase}'.");
            }

            await CreateDatabaseAsync(adminConnection, targetDatabase);
            CliConsole.Success($"Created target database '{targetDatabase}'.");
        }

        NpgsqlConnection.ClearAllPools();

        var bootstrapper = new PostgresTargetBootstrapper();
        await bootstrapper.BootstrapFreshDatabaseAsync(targetConnectionString);

        CliConsole.Success("Target schema is ready.");
        return 0;
    }

    private static async Task<NpgsqlConnection> OpenTargetConnectionAsync(string targetConnectionString)
    {
        // Enable error detail so FK/constraint violations include the offending value in the message.
        // Npgsql redacts this by default because it may contain PII, but during a migration run
        // the detail is essential for diagnosing failures.
        var builder = new NpgsqlConnectionStringBuilder(targetConnectionString)
        {
            IncludeErrorDetail = true
        };
        var targetConnection = new NpgsqlConnection(builder.ConnectionString);
        await targetConnection.OpenAsync();
        return targetConnection;
    }

    private static async Task<CommandLineOptions> RecreateTargetIfUserRequestsItAsync(
        SqlConnection sourceConnection,
        NpgsqlConnection targetConnection,
        CommandLineOptions options)
    {
        var sourceTables = GetCandidateTables(await SchemaDiscovery.GetSqlServerTablesAsync(sourceConnection));
        var targetTables = GetCandidateTables(await SchemaDiscovery.GetPostgresTablesAsync(targetConnection));
        var matching = SchemaDiscovery.AnalyzeTableMatches(sourceTables, targetTables);
        var compatibility = await AssessTableCompatibilityAsync(sourceConnection, targetConnection, matching.Matches);

        if (targetTables.Count == 0 || matching.Matches.Count == 0 || matching.Errors.Count > 0 || compatibility.Errors.Count > 0)
        {
            CliConsole.Warning("Target database does not contain the canonical table structure required for migration.");
            var createTarget = AnsiConsole.Confirm("Create a new empty target database with the required structure now?", true);
            if (!createTarget)
            {
                return options;
            }

            NpgsqlConnection.ClearPool(targetConnection);
            await targetConnection.CloseAsync();
            await targetConnection.DisposeAsync();
            NpgsqlConnection.ClearAllPools();

            var createStructureResult = await RunPrepareTargetAsync(options.TargetConnectionString, requireConfirmation: false);
            if (createStructureResult != 0)
            {
                throw new CommandLineException("Failed to create the target PostgreSQL database structure.");
            }

            return options with
            {
                AllowNonEmptyTarget = false
            };
        }

        if (options.AllowNonEmptyTarget)
        {
            return options;
        }

        var nonEmptyTable = await FindFirstNonEmptyPostgresTableAsync(targetConnection, targetTables);
        if (nonEmptyTable is null)
        {
            return options;
        }

        CliConsole.Warning($"Target database is not empty. First non-empty migration table: {nonEmptyTable}.");
        var recreate = AnsiConsole.Confirm("Recreate the target database as a new empty database with the required structure?", true);
        if (!recreate)
        {
            return options;
        }

        NpgsqlConnection.ClearPool(targetConnection);
        await targetConnection.CloseAsync();
        await targetConnection.DisposeAsync();
        NpgsqlConnection.ClearAllPools();

        var prepareResult = await RunPrepareTargetAsync(options.TargetConnectionString, requireConfirmation: false);
        if (prepareResult != 0)
        {
            throw new CommandLineException("Failed to recreate the target PostgreSQL database.");
        }

        return options with
        {
            AllowNonEmptyTarget = false
        };
    }

    private static bool IsMissingDatabase(PostgresException exception)
    {
        return string.Equals(exception.SqlState, PostgresErrorCodes.InvalidCatalogName, StringComparison.Ordinal);
    }

    private static async Task<int> RunExecuteAsync(SqlConnection sourceConnection, NpgsqlConnection targetConnection, CommandLineOptions options)
    {
        CliConsole.Info("Copying data...");

        await MigrationJournal.EnsureJournalTableAsync(targetConnection);

        var sourceTables = GetCandidateTables(await SchemaDiscovery.GetSqlServerTablesAsync(sourceConnection));
        var targetTables = GetCandidateTables(await SchemaDiscovery.GetPostgresTablesAsync(targetConnection));
        var matching = SchemaDiscovery.AnalyzeTableMatches(sourceTables, targetTables);
        var compatibility = await AssessTableCompatibilityAsync(sourceConnection, targetConnection, matching.Matches);
        var targetForeignKeys = await SchemaDiscovery.GetPostgresForeignKeysAsync(targetConnection);
        var selfReferenceColumnsByTargetTable = GetSelfReferenceColumnsByTargetTable(targetForeignKeys, matching.Matches);
        var nonSelfFksByTargetTable = targetForeignKeys
            .Where(fk => fk.ChildTable != fk.ParentTable)
            .GroupBy(fk => fk.ChildTable)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<PostgresForeignKeyDefinition>)group.ToList());

        RenderIssues("Execution warnings", CreateSelfReferenceWarnings(selfReferenceColumnsByTargetTable));
        RenderIssues("Execution issues", matching.Errors.Concat(compatibility.Errors));

        if (matching.Matches.Count == 0)
        {
            CliConsole.Error("Migration aborted: no compatible source/target table matches were found for the migration candidates.");
            return -1;
        }

        if (matching.Errors.Count > 0 || compatibility.Errors.Count > 0)
        {
            CliConsole.Error("Migration aborted: source and target schemas are not compatible for a safe copy.");
            return -2;
        }

        var sourceTableLookup = matching.Matches.ToDictionary(match => match.Source, match => match.Target);
        var targetToSourceLookup = matching.Matches.ToDictionary(match => match.Target, match => match.Source);

        // Use SQL Server FK edges as the primary ordering input, but supplement them with edges
        // derived from the PostgreSQL target schema. Post-EF6 migrations may have added FKs in the
        // target that don't exist in the SQL Server source, so the sort would otherwise miss those
        // dependencies and process child tables before their parents.
        var sqlServerForeignKeys = await SchemaDiscovery.GetSqlServerForeignKeyEdgesAsync(sourceConnection);
        var postgresEdgesAsSqlServer = targetForeignKeys
            .Where(fk => fk.ChildTable != fk.ParentTable)
            .Where(fk => targetToSourceLookup.ContainsKey(fk.ChildTable) && targetToSourceLookup.ContainsKey(fk.ParentTable))
            .Select(fk => (Child: targetToSourceLookup[fk.ChildTable], Parent: targetToSourceLookup[fk.ParentTable]));
        var foreignKeys = sqlServerForeignKeys.Concat(postgresEdgesAsSqlServer).Distinct().ToList();

        var orderedTables = SchemaDiscovery.TopologicalSortTables(matching.Matches.Select(match => match.Source).ToList(), foreignKeys);

        CliConsole.Info($"Tables scheduled for migration: {orderedTables.Count}");

        // Always disable FK checks for the duration of the copy. The topological sort cannot
        // guarantee a perfect ordering across FK cycles, and session_replication_role=replica
        // is the correct PostgreSQL mechanism for bulk loads. It is reset unconditionally in
        // the finally block below, so a failed run leaves no permanent state on the target.
        await SetPostgresSessionReplicationRoleAsync(targetConnection, "replica");
        CliConsole.Info("Foreign key checks disabled for this session (session_replication_role=replica).");

        // Drop non-PK unique indexes and unique constraints before copying data.
        // session_replication_role=replica bypasses FK checks but NOT unique indexes/constraints —
        // PostgreSQL enforces uniqueness via B-tree unconditionally. EF6 null-sentinel values
        // (e.g. OrganizationId=0 on multiple rows) would otherwise cause duplicate-key failures.
        // The indexes/constraints are recreated after the copy; a recreation failure is reported
        // as a data-quality warning rather than a migration failure.
        var targetTableSet = matching.Matches.Select(m => m.Target).ToList();
        var uniqueIndexes = await SchemaDiscovery.GetPostgresUniqueIndexesAsync(targetConnection, targetTableSet);
        var uniqueConstraints = await SchemaDiscovery.GetPostgresUniqueConstraintsAsync(targetConnection, targetTableSet);

        var skippedUniqueIndexes = new HashSet<(string Schema, string Name, string IndexName)>();

        foreach (var idx in uniqueIndexes)
        {
            try
            {
                var dropSql = $"DROP INDEX IF EXISTS {SchemaDiscovery.QuotePostgresIdentifier(idx.Table.Schema)}.{SchemaDiscovery.QuotePostgresIdentifier(idx.IndexName)};";
                await using var cmd = new NpgsqlCommand(dropSql, targetConnection);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == "2BP01")
            {
                skippedUniqueIndexes.Add((idx.Table.Schema, idx.Table.Name, idx.IndexName));
                CliConsole.Warning($"Skipping drop of unique index {idx.Table}.{idx.IndexName} because dependent object(s) exist.");
            }
        }

        var skippedUniqueConstraints = new HashSet<(string Schema, string Name, string ConstraintName)>();

        foreach (var uc in uniqueConstraints)
        {
            try
            {
                var dropSql = $"ALTER TABLE {SchemaDiscovery.QualifyPostgresTable(uc.Table)} DROP CONSTRAINT IF EXISTS {SchemaDiscovery.QuotePostgresIdentifier(uc.ConstraintName)};";
                await using var cmd = new NpgsqlCommand(dropSql, targetConnection);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (PostgresException ex) when (ex.SqlState == "2BP01")
            {
                skippedUniqueConstraints.Add((uc.Table.Schema, uc.Table.Name, uc.ConstraintName));
                CliConsole.Warning($"Skipping drop of unique constraint {uc.Table}.{uc.ConstraintName} because dependent object(s) exist.");
            }
        }

        var droppedUniqueIndexCount = uniqueIndexes.Count - skippedUniqueIndexes.Count;
        var droppedUniqueConstraintCount = uniqueConstraints.Count - skippedUniqueConstraints.Count;

        if (uniqueIndexes.Count + uniqueConstraints.Count > 0)
        {
            CliConsole.Info($"Dropped {droppedUniqueIndexCount} unique index(es) and {droppedUniqueConstraintCount} unique constraint(s) for the duration of the copy.");
        }

        var failures = new List<string>();
        var results = new List<TableExecutionResult>();

        try
        {
            await AnsiConsole.Progress()
                .AutoRefresh(true)
                .AutoClear(false)
                .Columns(
                [
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new SpinnerColumn()
                ])
                .StartAsync(async context =>
            {
                var overallTask = context.AddTask("[green]Migrating tables[/]", maxValue: orderedTables.Count == 0 ? 1 : orderedTables.Count);

                foreach (var sourceTable in orderedTables)
                {
                    var targetTable = sourceTableLookup[sourceTable];
                    overallTask.Description = $"[green]Processing {Markup.Escape(sourceTable.ToString())}[/]";

                    try
                    {
                        CliConsole.Info($"Mapping: {sourceTable} → {targetTable}");
                        await MigrationJournal.MarkStartedAsync(targetConnection, targetTable);
                        selfReferenceColumnsByTargetTable.TryGetValue(targetTable, out var selfReferenceColumns);
                        nonSelfFksByTargetTable.TryGetValue(targetTable, out var tableForeignKeys);
                        var sourceRowCount = await CountRowsSqlServerAsync(sourceConnection, sourceTable);
                        var rowsCopied = await CopyTableAsync(sourceConnection, targetConnection, sourceTable, targetTable, selfReferenceColumns ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase), tableForeignKeys ?? []);
                        await ReseedSequenceIfPresentAsync(targetConnection, targetTable);
                        await MigrationJournal.MarkSucceededAsync(targetConnection, targetTable, rowsCopied);
                        results.Add(new TableExecutionResult(sourceTable, TableExecutionStatus.Succeeded, sourceRowCount, rowsCopied));
                    }
                    catch (Exception exception)
                    {
                        await MigrationJournal.MarkFailedAsync(targetConnection, targetTable, exception.Message);
                        var message = $"Failed to migrate {sourceTable} -> {targetTable}: {exception.Message}";
                        failures.Add(message);
                        results.Add(new TableExecutionResult(sourceTable, TableExecutionStatus.Failed, null, null, exception.Message));
                        CliConsole.Error(message);

                        if (!options.ContinueOnError)
                        {
                            CliConsole.Warning("Stopping on first error. Use --continue-on-error to continue with remaining tables.");
                            overallTask.Increment(1);
                            return;
                        }
                    }

                    overallTask.Increment(1);
                }
            });
        }
        finally
        {
            await SetPostgresSessionReplicationRoleAsync(targetConnection, "origin");
        }

        // Recreate unique indexes and constraints now that all data is in place.
        // Failures here indicate genuine duplicate data in the source (EF6 null-sentinel
        // collisions or real duplicates). They are reported as warnings so the rest of the
        // migration result is still visible; the caller should investigate before going live.
        var indexFailures = new List<string>();

        foreach (var idx in uniqueIndexes)
        {
            if (skippedUniqueIndexes.Contains((idx.Table.Schema, idx.Table.Name, idx.IndexName)))
            {
                continue;
            }

            try
            {
                await using var cmd = new NpgsqlCommand(idx.IndexDdl, targetConnection);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                var msg = $"Could not recreate unique index {idx.Table}.{idx.IndexName}: {ex.Message}";
                indexFailures.Add(msg);
                CliConsole.Warning(msg);
            }
        }

        foreach (var uc in uniqueConstraints)
        {
            if (skippedUniqueConstraints.Contains((uc.Table.Schema, uc.Table.Name, uc.ConstraintName)))
            {
                continue;
            }

            try
            {
                var cols = string.Join(", ", uc.QuotedColumns);
                var addSql = $"ALTER TABLE {SchemaDiscovery.QualifyPostgresTable(uc.Table)} ADD CONSTRAINT {SchemaDiscovery.QuotePostgresIdentifier(uc.ConstraintName)} UNIQUE ({cols});";
                await using var cmd = new NpgsqlCommand(addSql, targetConnection);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                var msg = $"Could not recreate unique constraint {uc.Table}.{uc.ConstraintName}: {ex.Message}";
                indexFailures.Add(msg);
                CliConsole.Warning(msg);
            }
        }

        if (indexFailures.Count > 0)
        {
            CliConsole.Warning($"{indexFailures.Count} unique index/constraint(s) could not be recreated due to duplicate source data. Investigate these columns before going live.");
        }
        else if (uniqueIndexes.Count + uniqueConstraints.Count > 0)
        {
            CliConsole.Success($"Recreated {droppedUniqueIndexCount} unique index(es) and {droppedUniqueConstraintCount} unique constraint(s).");
        }

        CliConsole.RenderExecutionSummary(results);

        if (failures.Count > 0)
        {
            CliConsole.Error($"Migration completed with {failures.Count} failed table(s).");

            if (!options.ContinueOnError)
            {
                return -3;
            }

            return -4;
        }

        CliConsole.Success("Migration completed successfully.");
        return 0;
    }

    private static async Task<int> RunValidateAsync(SqlConnection sourceConnection, NpgsqlConnection targetConnection)
    {
        CliConsole.Info("Validating row counts...");

        var sourceTables = GetCandidateTables(await SchemaDiscovery.GetSqlServerTablesAsync(sourceConnection));
        var targetTables = GetCandidateTables(await SchemaDiscovery.GetPostgresTablesAsync(targetConnection));
        var matching = SchemaDiscovery.AnalyzeTableMatches(sourceTables, targetTables);
        var compatibility = await AssessTableCompatibilityAsync(sourceConnection, targetConnection, matching.Matches);

        RenderIssues("Validation warnings", matching.Warnings.Concat(compatibility.Warnings));
        RenderIssues("Validation issues", matching.Errors.Concat(compatibility.Errors));

        if (matching.Matches.Count == 0)
        {
            CliConsole.Error("Validation failed: no compatible source/target table matches were found for the migration candidates.");
            return -1;
        }

        if (matching.Errors.Count > 0 || compatibility.Errors.Count > 0)
        {
            CliConsole.Error("Validation failed: source and target schemas are not compatible for a trustworthy comparison.");
            return -2;
        }

        var mismatches = new List<ValidationMismatch>();
        var comparedTableCount = 0;
        foreach (var match in matching.Matches.OrderBy(match => match.Source.Schema).ThenBy(match => match.Source.Name))
        {
            comparedTableCount++;
            var sourceCount = await CountRowsSqlServerAsync(sourceConnection, match.Source);
            var targetCount = await CountRowsPostgresAsync(targetConnection, match.Target);

            if (sourceCount != targetCount)
            {
                mismatches.Add(new ValidationMismatch(match.Source, sourceCount, targetCount));
            }
        }

        CliConsole.RenderValidationSummary(comparedTableCount, mismatches);

        if (mismatches.Count > 0)
        {
            CliConsole.Error("Validation failed: row count mismatches found.");
            return -3;
        }

        CliConsole.Success("Validation passed.");
        return 0;
    }

    private static async Task<long> CopyTableAsync(
        SqlConnection sourceConnection,
        NpgsqlConnection targetConnection,
        TableRef sourceTable,
        TableRef targetTable,
        IReadOnlySet<string> deferredSelfReferenceTargetColumns,
        IReadOnlyList<PostgresForeignKeyDefinition> tableForeignKeys)
    {
        var copyPlan = await BuildColumnCopyPlanAsync(sourceConnection, targetConnection, sourceTable, targetTable, deferredSelfReferenceTargetColumns, tableForeignKeys);

        var selectColumns = string.Join(", ", copyPlan.InsertSourceSelectExpressions);
        var insertColumns = string.Join(", ", copyPlan.InsertTargetColumnNames.Select(SchemaDiscovery.QuotePostgresIdentifier));
        var parameterList = string.Join(", ", Enumerable.Range(0, copyPlan.InsertTargetColumnNames.Count).Select(index => $"@p{index}"));

        var selectSql = $"SELECT {selectColumns} FROM {SchemaDiscovery.QualifySqlServerTable(sourceTable)};";
        var insertSql = $"INSERT INTO {SchemaDiscovery.QualifyPostgresTable(targetTable)} ({insertColumns}) VALUES ({parameterList});";

        await using var transaction = await targetConnection.BeginTransactionAsync();

        // The source reader must be fully consumed and closed before ApplyDeferredSelfReferencesAsync
        // opens a second SqlCommand on the same sourceConnection. SQL Server does not allow two
        // concurrent open readers on a single connection without MARS enabled.
        long copiedRows;
        {
            await using var selectCommand = new SqlCommand(selectSql, sourceConnection);
            await using var reader = await selectCommand.ExecuteReaderAsync();
            await using var insertCommand = new NpgsqlCommand(insertSql, targetConnection, transaction);

            for (var index = 0; index < copyPlan.InsertTargetColumnNames.Count; index++)
            {
                insertCommand.Parameters.Add(new NpgsqlParameter($"@p{index}", DBNull.Value));
            }

            copiedRows = 0;
            const long progressLogInterval = 100;
            while (await reader.ReadAsync())
            {
                for (var index = 0; index < copyPlan.InsertTargetColumnNames.Count; index++)
                {
                    insertCommand.Parameters[index].Value = reader.IsDBNull(index) ? DBNull.Value : reader.GetValue(index);
                }

                await insertCommand.ExecuteNonQueryAsync();
                copiedRows++;
                if (copiedRows % progressLogInterval == 0)
                {
                    CliConsole.Info($"{targetTable}: inserted {copiedRows:N0} rows...");
                }
            }
        } // reader and selectCommand are disposed here, freeing sourceConnection for reuse

        if (copyPlan.DeferredSelfReferenceUpdate is not null)
        {
            await ApplyDeferredSelfReferencesAsync(sourceConnection, targetConnection, transaction, sourceTable, targetTable, copyPlan.DeferredSelfReferenceUpdate);
        }

        await transaction.CommitAsync();
        return copiedRows;
    }

    private static async Task ApplyDeferredSelfReferencesAsync(
        SqlConnection sourceConnection,
        NpgsqlConnection targetConnection,
        NpgsqlTransaction transaction,
        TableRef sourceTable,
        TableRef targetTable,
        DeferredSelfReferenceUpdatePlan updatePlan)
    {
        CliConsole.Info($"{targetTable}: starting deferred self-reference updates...");
        var deferredSelectExpressions = updatePlan.DeferredColumns.Select(column => column.SelectExpression).ToList();
        var primaryKeySelectExpressions = updatePlan.PrimaryKeyColumns.Select(column => column.SelectExpression).ToList();
        var selectSql = $"SELECT {string.Join(", ", deferredSelectExpressions.Concat(primaryKeySelectExpressions))} FROM {SchemaDiscovery.QualifySqlServerTable(sourceTable)};";

        var assignments = string.Join(", ", updatePlan.DeferredColumns.Select((column, index) => $"{SchemaDiscovery.QuotePostgresIdentifier(column.TargetColumnName)} = @value{index}"));
        var predicates = string.Join(" AND ", updatePlan.PrimaryKeyTargetColumnNames.Select((columnName, index) => $"{SchemaDiscovery.QuotePostgresIdentifier(columnName)} = @key{index}"));
        var updateSql = $"UPDATE {SchemaDiscovery.QualifyPostgresTable(targetTable)} SET {assignments} WHERE {predicates};";

        await using var selectCommand = new SqlCommand(selectSql, sourceConnection);
        await using var reader = await selectCommand.ExecuteReaderAsync();
        await using var updateCommand = new NpgsqlCommand(updateSql, targetConnection, transaction);

        for (var index = 0; index < updatePlan.DeferredColumns.Count; index++)
        {
            updateCommand.Parameters.Add(new NpgsqlParameter($"@value{index}", DBNull.Value));
        }

        for (var index = 0; index < updatePlan.PrimaryKeyColumns.Count; index++)
        {
            updateCommand.Parameters.Add(new NpgsqlParameter($"@key{index}", DBNull.Value));
        }

        long updatedRows = 0;
        const long progressLogInterval = 100;
        while (await reader.ReadAsync())
        {
            for (var index = 0; index < updatePlan.DeferredColumns.Count; index++)
            {
                updateCommand.Parameters[index].Value = reader.IsDBNull(index) ? DBNull.Value : reader.GetValue(index);
            }

            for (var index = 0; index < updatePlan.PrimaryKeyColumns.Count; index++)
            {
                var readerIndex = updatePlan.DeferredColumns.Count + index;
                updateCommand.Parameters[updatePlan.DeferredColumns.Count + index].Value = reader.IsDBNull(readerIndex) ? DBNull.Value : reader.GetValue(readerIndex);
            }

            await updateCommand.ExecuteNonQueryAsync();
            updatedRows++;
            if (updatedRows % progressLogInterval == 0)
            {
                CliConsole.Info($"{targetTable}: applied {updatedRows:N0} deferred self-reference updates...");
            }
        }

        CliConsole.Info($"{targetTable}: deferred self-reference updates completed ({updatedRows:N0} rows).");
    }

    private static async Task<long> CountRowsSqlServerAsync(SqlConnection sourceConnection, TableRef table)
    {
        var sql = $"SELECT COUNT_BIG(1) FROM {SchemaDiscovery.QualifySqlServerTable(table)};";
        await using var command = new SqlCommand(sql, sourceConnection);
        var value = await command.ExecuteScalarAsync();
        return Convert.ToInt64(value);
    }

    private static async Task<long> CountRowsPostgresAsync(NpgsqlConnection targetConnection, TableRef table)
    {
        var sql = $"SELECT COUNT(*) FROM {SchemaDiscovery.QualifyPostgresTable(table)};";
        await using var command = new NpgsqlCommand(sql, targetConnection);
        var value = await command.ExecuteScalarAsync();
        return Convert.ToInt64(value);
    }

    private static async Task<TableRef?> FindFirstNonEmptyPostgresTableAsync(NpgsqlConnection targetConnection, IReadOnlyCollection<TableRef> targetTables)
    {
        foreach (var table in targetTables)
        {
            var query = $"SELECT EXISTS (SELECT 1 FROM {SchemaDiscovery.QualifyPostgresTable(table)} LIMIT 1);";
            await using var command = new NpgsqlCommand(query, targetConnection);
            var exists = await command.ExecuteScalarAsync();
            if (exists is bool boolExists && boolExists)
            {
                return table;
            }
        }

        return null;
    }

    private static async Task ReseedSequenceIfPresentAsync(NpgsqlConnection targetConnection, TableRef table)
    {
        const string idColumnQuery = @"
SELECT column_name
FROM information_schema.columns
WHERE table_schema = @schemaName
  AND table_name = @tableName
  AND lower(column_name) = 'id'
LIMIT 1;";

        await using var idColumnCommand = new NpgsqlCommand(idColumnQuery, targetConnection);
        idColumnCommand.Parameters.AddWithValue("@schemaName", table.Schema);
        idColumnCommand.Parameters.AddWithValue("@tableName", table.Name);

        var idColumnValue = await idColumnCommand.ExecuteScalarAsync();
        if (idColumnValue is not string idColumnName || string.IsNullOrWhiteSpace(idColumnName))
        {
            return;
        }

        const string sequenceNameQuery = "SELECT pg_get_serial_sequence(@qualifiedTable, @idColumn);";
        await using var sequenceNameCommand = new NpgsqlCommand(sequenceNameQuery, targetConnection);
        var qualifiedTable = $"{SchemaDiscovery.QuotePostgresIdentifier(table.Schema)}.{SchemaDiscovery.QuotePostgresIdentifier(table.Name)}";
        sequenceNameCommand.Parameters.AddWithValue("@qualifiedTable", qualifiedTable);
        sequenceNameCommand.Parameters.AddWithValue("@idColumn", idColumnName);

        var sequenceNameValue = await sequenceNameCommand.ExecuteScalarAsync();
        if (sequenceNameValue is not string sequenceName || string.IsNullOrWhiteSpace(sequenceName))
        {
            return;
        }

        var reseedSql = $@"
SELECT setval(
    @sequenceName,
    COALESCE((SELECT MAX({SchemaDiscovery.QuotePostgresIdentifier(idColumnName)}) FROM {SchemaDiscovery.QualifyPostgresTable(table)}), 1),
    true
);";
        await using var reseedCommand = new NpgsqlCommand(reseedSql, targetConnection);
        reseedCommand.Parameters.AddWithValue("@sequenceName", sequenceName);
        await reseedCommand.ExecuteNonQueryAsync();
    }

    private static async Task SetPostgresSessionReplicationRoleAsync(NpgsqlConnection connection, string role)
    {
        var sql = $"SET session_replication_role = {role};";
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static async Task<bool> DatabaseExistsAsync(NpgsqlConnection adminConnection, string databaseName)
    {
        const string sql = "SELECT EXISTS (SELECT 1 FROM pg_database WHERE datname = @databaseName);";
        await using var command = new NpgsqlCommand(sql, adminConnection);
        command.Parameters.AddWithValue("@databaseName", databaseName);
        var value = await command.ExecuteScalarAsync();
        return value is true;
    }

    private static async Task DropDatabaseAsync(NpgsqlConnection adminConnection, string databaseName)
    {
        const string terminateSql = @"
SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE datname = @databaseName
  AND pid <> pg_backend_pid();";

        await using (var terminateCommand = new NpgsqlCommand(terminateSql, adminConnection))
        {
            terminateCommand.Parameters.AddWithValue("@databaseName", databaseName);
            await terminateCommand.ExecuteNonQueryAsync();
        }

        var dropSql = $"DROP DATABASE {SchemaDiscovery.QuotePostgresIdentifier(databaseName)};";
        await using var dropCommand = new NpgsqlCommand(dropSql, adminConnection);
        await dropCommand.ExecuteNonQueryAsync();
    }

    private static async Task CreateDatabaseAsync(NpgsqlConnection adminConnection, string databaseName)
    {
        var createSql = $"CREATE DATABASE {SchemaDiscovery.QuotePostgresIdentifier(databaseName)};";
        await using var createCommand = new NpgsqlCommand(createSql, adminConnection);
        await createCommand.ExecuteNonQueryAsync();
    }

    private static async Task<SchemaCompatibilityAssessment> AssessTableCompatibilityAsync(
        SqlConnection sourceConnection,
        NpgsqlConnection targetConnection,
        IReadOnlyCollection<TableMatch> matchedTables)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        var allTargetForeignKeys = await SchemaDiscovery.GetPostgresForeignKeysAsync(targetConnection);
        var fksByTargetTable = allTargetForeignKeys
            .GroupBy(fk => fk.ChildTable)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<PostgresForeignKeyDefinition>)group.ToList());

        var zeroFiltersByTargetTable = new Dictionary<TableRef, IReadOnlyList<string>>();

        foreach (var match in matchedTables.OrderBy(item => item.Source.Schema).ThenBy(item => item.Source.Name))
        {
            var sourceColumns = (await SchemaDiscovery.GetSqlServerColumnDefinitionsAsync(sourceConnection, match.Source))
                .Where(column => !column.IsComputed)
                .OrderBy(column => column.Ordinal)
                .ToList();

            var targetColumns = await SchemaDiscovery.GetPostgresColumnDefinitionsAsync(targetConnection, match.Target);
            fksByTargetTable.TryGetValue(match.Target, out var matchTableForeignKeys);
            // Pass an empty sentinel set so GUID warnings are suppressed during the assessment phase.
            // The transformation is still applied defensively in the copy phase; warnings are only
            // emitted there when Guid.Empty values are actually found in the source data.
            var plan = BuildColumnCompatibilityPlan(match.Source, match.Target, sourceColumns, targetColumns, null, matchTableForeignKeys, new HashSet<string>());
            errors.AddRange(plan.Errors);
            warnings.AddRange(plan.Warnings);
        }

        return new SchemaCompatibilityAssessment(errors, warnings);
    }

    private static async Task<ColumnCopyPlan> BuildColumnCopyPlanAsync(
        SqlConnection sourceConnection,
        NpgsqlConnection targetConnection,
        TableRef sourceTable,
        TableRef targetTable,
        IReadOnlySet<string> deferredSelfReferenceTargetColumns,
        IReadOnlyList<PostgresForeignKeyDefinition> tableForeignKeys)
    {
        var sourceColumns = (await SchemaDiscovery.GetSqlServerColumnDefinitionsAsync(sourceConnection, sourceTable))
            .Where(column => !column.IsComputed)
            .OrderBy(column => column.Ordinal)
            .ToList();

        if (sourceColumns.Count == 0)
        {
            throw new InvalidOperationException($"No non-computed source columns found for table {sourceTable}.");
        }

        var targetColumns = await SchemaDiscovery.GetPostgresColumnDefinitionsAsync(targetConnection, targetTable);

        // For each NOT NULL uniqueidentifier source column, check whether any row actually contains
        // Guid.Empty. Only columns with real sentinel values get the CASE WHEN / NULLIF treatment.
        // This avoids false-positive warnings on tables where Guid.Empty never appears.
        var guidSentinelSourceColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var guidColumns = sourceColumns
            .Where(c => IsGuidType(c.DataType))
            .ToList();
        if (guidColumns.Count > 0)
        {
            const string emptyGuid = "'00000000-0000-0000-0000-000000000000'";
            var predicates = guidColumns
                .Select(c => $"{SchemaDiscovery.QuoteSqlServerIdentifier(c.Name)} = {emptyGuid}")
                .ToList();
            var checkSql = $"SELECT TOP 1 1 FROM {SchemaDiscovery.QualifySqlServerTable(sourceTable)} WHERE {string.Join(" OR ", predicates)}";
            await using var checkCmd = new SqlCommand(checkSql, sourceConnection);
            var hasAny = await checkCmd.ExecuteScalarAsync();
            if (hasAny is not null)
            {
                // At least one column has a Guid.Empty sentinel: do per-column checks to find which ones.
                foreach (var guidCol in guidColumns)
                {
                    var colCheckSql = $"SELECT TOP 1 1 FROM {SchemaDiscovery.QualifySqlServerTable(sourceTable)} WHERE {SchemaDiscovery.QuoteSqlServerIdentifier(guidCol.Name)} = {emptyGuid}";
                    await using var colCmd = new SqlCommand(colCheckSql, sourceConnection);
                    if (await colCmd.ExecuteScalarAsync() is not null)
                    {
                        guidSentinelSourceColumns.Add(guidCol.Name);
                    }
                }
            }
        }

        var compatibilityPlan = BuildColumnCompatibilityPlan(sourceTable, targetTable, sourceColumns, targetColumns, deferredSelfReferenceTargetColumns, tableForeignKeys, guidSentinelSourceColumns);
        if (compatibilityPlan.Errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(" ", compatibilityPlan.Errors));
        }

        DeferredSelfReferenceUpdatePlan? deferredSelfReferenceUpdate = null;
        if (compatibilityPlan.DeferredSelfReferenceMappings.Count > 0)
        {
            var primaryKeyColumns = await SchemaDiscovery.GetPostgresPrimaryKeyColumnsAsync(targetConnection, targetTable);
            if (primaryKeyColumns.Count == 0)
            {
                throw new InvalidOperationException($"Target table {targetTable} has deferred self-reference column(s), but no primary key was discovered for post-insert updates.");
            }

            var sourceLookup = sourceColumns.ToDictionary(column => column.Name, StringComparer.OrdinalIgnoreCase);
            var primaryKeyMappings = new List<ColumnValueMapping>();
            var usedPrimaryKeySourceColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var primaryKeyColumnNameSet = primaryKeyColumns.ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var primaryKeyColumn in primaryKeyColumns)
            {
                if (!TryResolveSourceColumn(targetTable, primaryKeyColumn, sourceLookup, usedPrimaryKeySourceColumns, primaryKeyColumnNameSet, out var primaryKeyMapping))
                {
                    throw new InvalidOperationException($"Target table {targetTable} primary key column {primaryKeyColumn} has no compatible SQL Server source column.");
                }

                foreach (var usedSourceColumn in primaryKeyMapping.UsedSourceColumns)
                {
                    usedPrimaryKeySourceColumns.Add(usedSourceColumn);
                }

                primaryKeyMappings.Add(primaryKeyMapping);
            }

            deferredSelfReferenceUpdate = new DeferredSelfReferenceUpdatePlan(
                compatibilityPlan.DeferredSelfReferenceMappings,
                primaryKeyMappings,
                primaryKeyColumns);
        }

        return new ColumnCopyPlan(
            compatibilityPlan.InsertSourceSelectExpressions,
            compatibilityPlan.InsertTargetColumnNames,
            deferredSelfReferenceUpdate);
    }

    private static bool IsRequiredWithoutSourceValue(PostgresColumnDefinition column)
    {
        return !column.IsNullable && !column.HasDefaultValue && !column.IsIdentity && !column.IsGenerated;
    }

    private static bool IsIntegerType(string sqlServerDataType)
    {
        return sqlServerDataType is "int" or "bigint" or "smallint" or "tinyint";
    }

    private static bool IsGuidType(string sqlServerDataType)
    {
        return sqlServerDataType is "uniqueidentifier";
    }

    private static bool IsAdviceSentsOwnerBackfillTarget(TableRef targetTable, string targetColumnName)
    {
        return targetTable.Schema.Equals("dbo", StringComparison.OrdinalIgnoreCase)
               && targetTable.Name.Equals("AdviceSents", StringComparison.OrdinalIgnoreCase)
               && (targetColumnName.Equals("ObjectOwnerId", StringComparison.OrdinalIgnoreCase)
                   || targetColumnName.Equals("LastChangedByUserId", StringComparison.OrdinalIgnoreCase));
    }

    private static string GetAdviceSentsOwnerBackfillExpression(string targetColumnName)
    {
        var targetColumn = SchemaDiscovery.QuoteSqlServerIdentifier(targetColumnName);
        const string adviceOwnerLookup = "(SELECT TOP 1 [ObjectOwnerId] FROM [dbo].[Advice] WHERE [dbo].[Advice].[Id] = [dbo].[AdviceSents].[AdviceId])";
        return $"COALESCE({targetColumn}, {adviceOwnerLookup})";
    }

    private static Dictionary<TableRef, HashSet<string>> GetSelfReferenceColumnsByTargetTable(
        IReadOnlyCollection<PostgresForeignKeyDefinition> targetForeignKeys,
        IReadOnlyCollection<TableMatch> matches)
    {
        var matchedTargetTables = matches
            .Select(match => match.Target)
            .ToHashSet();

        return targetForeignKeys
            .Where(foreignKey => foreignKey.ChildTable == foreignKey.ParentTable)
            .Where(foreignKey => matchedTargetTables.Contains(foreignKey.ChildTable))
            .GroupBy(foreignKey => foreignKey.ChildTable)
            .ToDictionary(
                group => group.Key,
                group => group.SelectMany(foreignKey => foreignKey.ChildColumns).ToHashSet(StringComparer.OrdinalIgnoreCase));
    }

    private static IEnumerable<string> CreateSelfReferenceWarnings(IReadOnlyDictionary<TableRef, HashSet<string>> selfReferenceColumnsByTargetTable)
    {
        foreach (var table in selfReferenceColumnsByTargetTable.Keys.OrderBy(table => table.Schema).ThenBy(table => table.Name))
        {
            var columns = selfReferenceColumnsByTargetTable[table].OrderBy(column => column, StringComparer.OrdinalIgnoreCase);
            yield return $"Target table {table} has self-referencing foreign key column(s) that require post-insert updates: {string.Join(", ", columns)}.";
        }
    }

    private static ColumnCompatibilityPlan BuildColumnCompatibilityPlan(
        TableRef sourceTable,
        TableRef targetTable,
        IReadOnlyList<SqlServerColumnDefinition> sourceColumns,
        IReadOnlyList<PostgresColumnDefinition> targetColumns,
        IReadOnlySet<string>? deferredSelfReferenceTargetColumns = null,
        IReadOnlyCollection<PostgresForeignKeyDefinition>? targetTableForeignKeys = null,
        IReadOnlySet<string>? guidEmptySentinelSourceColumns = null)
    {
        // Nullable integer FK columns whose source value may be 0 (EF6 null sentinel) rather than a real FK.
        // NULLIF(col, 0) in the SELECT converts those zeros to NULL so the PostgreSQL FK constraint is satisfied.
        var nullableIntFkColumns = targetTableForeignKeys is null
            ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : targetTableForeignKeys
                .Where(fk => fk.ChildTable != fk.ParentTable)
                .SelectMany(fk => fk.ChildColumns)
                .Intersect(
                    targetColumns.Where(col => col.IsNullable).Select(col => col.Name),
                    StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Non-nullable integer FK columns where 0 is an EF6 null sentinel but the target requires a real parent.
        // With session_replication_role=replica in effect, FK constraints are bypassed during the copy, so we
        // allow these rows through with value 0. Excluding them would silently discard large amounts of data
        // (e.g. every Organization whose TypeId was never set). Orphaned FK references are a known and accepted
        // consequence of migrating EF6 data; they can be investigated and remediated post-migration.
        var nonNullableIntFkColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var errors = new List<string>();
        var warnings = new List<string>();
        var appliedCompatibilityAliases = new List<string>();
        var requiredFkZeroFilterSourceExpressions = new List<string>();
        var deferredSelfReferenceColumns = new List<string>();
        var deferredSelfReferenceMappings = new List<ColumnValueMapping>();
        var requiredTargetOnlyColumns = new List<string>();
        var optionalTargetOnlyColumns = new List<string>();

        var sourceLookup = sourceColumns.ToDictionary(column => column.Name, StringComparer.OrdinalIgnoreCase);
        var usedSourceColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var targetColumnNameSet = targetColumns.Select(c => c.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var sourceSelectExpressions = new List<string>();
        var targetColumnNames = new List<string>();

        foreach (var targetColumn in targetColumns.OrderBy(column => column.Ordinal))
        {
            if (TryResolveSourceColumn(targetTable, targetColumn.Name, sourceLookup, usedSourceColumns, targetColumnNameSet, out var sourceColumn))
            {
                foreach (var usedSourceColumn in sourceColumn.UsedSourceColumns)
                {
                    usedSourceColumns.Add(usedSourceColumn);
                }

                if (deferredSelfReferenceTargetColumns?.Contains(targetColumn.Name) == true && !IsRequiredWithoutSourceValue(targetColumn))
                {
                    deferredSelfReferenceColumns.Add(targetColumn.Name);
                    deferredSelfReferenceMappings.Add(sourceColumn);
                    continue;
                }

                var primarySourceColumn = sourceLookup.GetValueOrDefault(sourceColumn.UsedSourceColumns.FirstOrDefault() ?? string.Empty);
                var selectExpression = sourceColumn.SelectExpression;

                if (nullableIntFkColumns.Contains(targetColumn.Name))
                {
                    if (primarySourceColumn is not null && IsIntegerType(primarySourceColumn.DataType))
                    {
                        selectExpression = $"NULLIF({selectExpression}, 0)";
                        // zero-to-null coercion is silent expected behaviour for all EF6 nullable FK columns
                    }
                }
                else if (nonNullableIntFkColumns.Contains(targetColumn.Name))
                {
                    if (primarySourceColumn is not null && IsIntegerType(primarySourceColumn.DataType))
                    {
                        // Rows with source value 0 are excluded via WHERE; 0 cannot be nullified (NOT NULL) and
                        // doesn't reference a real parent row. The raw selectExpression is used (not the NULLIF form).
                        requiredFkZeroFilterSourceExpressions.Add(sourceColumn.SelectExpression);
                        appliedCompatibilityAliases.Add($"{targetColumn.Name}: rows with source value 0 excluded (EF6 null-sentinel for required FK)");
                    }
                }
                else if (primarySourceColumn is not null && IsGuidType(primarySourceColumn.DataType))
                {
                    // Guid.Empty (all-zeros UUID) is an EF6 null sentinel for uninitialized GUID properties.
                    // Multiple rows may share the same Guid.Empty, violating unique constraints.
                    // The transformation is always applied defensively. A warning is only emitted when
                    // guidEmptySentinelSourceColumns indicates that Guid.Empty actually exists in the data
                    // (null means the assessment phase, which warns conservatively without a data scan).
                    const string emptyGuid = "'00000000-0000-0000-0000-000000000000'";
                    var hasGuidSentinel = guidEmptySentinelSourceColumns is null
                        || guidEmptySentinelSourceColumns.Contains(primarySourceColumn.Name);

                    if (targetColumn.IsNullable)
                    {
                        selectExpression = $"NULLIF({selectExpression}, {emptyGuid})";
                        if (hasGuidSentinel)
                        {
                            appliedCompatibilityAliases.Add($"{targetColumn.Name}: Guid.Empty-to-null coercion applied (EF6 null-sentinel)");
                        }
                    }
                    else
                    {
                        // NOT NULL: replace Guid.Empty with a fresh unique UUID so the row is preserved.
                        selectExpression = $"CASE WHEN ({selectExpression}) = {emptyGuid} THEN NEWID() ELSE ({selectExpression}) END";
                        if (hasGuidSentinel)
                        {
                            appliedCompatibilityAliases.Add($"{targetColumn.Name}: Guid.Empty replaced with NEWID() (EF6 null-sentinel)");
                        }
                    }
                }

                if (IsAdviceSentsOwnerBackfillTarget(targetTable, targetColumn.Name))
                {
                    selectExpression = GetAdviceSentsOwnerBackfillExpression(targetColumn.Name);
                    appliedCompatibilityAliases.Add($"{targetColumn.Name}: null values backfilled from Advice.ObjectOwnerId");
                }

                sourceSelectExpressions.Add(selectExpression);
                targetColumnNames.Add(targetColumn.Name);

                if (!string.IsNullOrWhiteSpace(sourceColumn.CompatibilityReason))
                {
                    appliedCompatibilityAliases.Add($"{targetColumn.Name} <- {sourceColumn.DisplayName}");
                }

                continue;
            }

            if (IsRequiredWithoutSourceValue(targetColumn))
            {
                requiredTargetOnlyColumns.Add(targetColumn.Name);
            }
            else
            {
                optionalTargetOnlyColumns.Add(targetColumn.Name);
            }
        }

        if (requiredTargetOnlyColumns.Count > 0)
        {
            errors.Add($"Target table {targetTable} has required column(s) with no source counterpart or default value: {string.Join(", ", requiredTargetOnlyColumns)}.");
        }

        if (optionalTargetOnlyColumns.Count > 0)
        {
            warnings.Add($"Target table {targetTable} has target-only column(s) that will be populated by defaults/nullability: {string.Join(", ", optionalTargetOnlyColumns)}.");
        }

        if (appliedCompatibilityAliases.Count > 0)
        {
            warnings.Add($"Target table {targetTable} will use legacy SQL Server source column mapping(s): {string.Join(", ", appliedCompatibilityAliases)}.");
        }

        if (deferredSelfReferenceColumns.Count > 0)
        {
            warnings.Add($"Target table {targetTable} has self-referencing column(s) that will be updated after initial row insert: {string.Join(", ", deferredSelfReferenceColumns)}.");
        }

        var ignoredSourceColumns = sourceColumns
            .Where(column => !usedSourceColumns.Contains(column.Name))
            .Select(column => column.Name)
            .ToList();

        if (ignoredSourceColumns.Count > 0)
        {
            warnings.Add($"Source table {sourceTable} has source-only column(s) that will be ignored: {string.Join(", ", ignoredSourceColumns)}.");
        }

        if (sourceSelectExpressions.Count == 0)
        {
            errors.Add($"No compatible source columns were found for source table {sourceTable} and target table {targetTable}.");
        }

        return new ColumnCompatibilityPlan(sourceSelectExpressions, targetColumnNames, deferredSelfReferenceMappings, errors, warnings);
    }

    private static bool TryResolveSourceColumn(
        TableRef targetTable,
        string targetColumnName,
        IReadOnlyDictionary<string, SqlServerColumnDefinition> sourceLookup,
        ISet<string> usedSourceColumns,
        IReadOnlySet<string> targetColumnNames,
        out ColumnValueMapping sourceColumn)
    {
        var exactSourceColumnExists = sourceLookup.TryGetValue(targetColumnName, out var exactSourceColumn)
                                      && !usedSourceColumns.Contains(exactSourceColumn.Name);

        foreach (var compatibilityAlias in LegacySqlServerSourceCompatibility.GetCompatibleSourceColumns(targetTable, targetColumnName))
        {
            if (!sourceLookup.TryGetValue(compatibilityAlias.SourceColumnName, out var compatibleSourceColumn)
                || usedSourceColumns.Contains(compatibleSourceColumn.Name))
            {
                continue;
            }

            var isConventionAlias = IsLegacyForeignKeyConventionReason(compatibilityAlias.Reason);

            // Don't consume a fallback source column via COALESCE if the target schema also has a
            // column with that exact name — the fallback column is reserved for its own exact match.
            if (targetColumnNames.Contains(compatibleSourceColumn.Name)
                && !string.Equals(compatibleSourceColumn.Name, targetColumnName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (exactSourceColumnExists)
            {
                sourceColumn = new ColumnValueMapping(
                    $"COALESCE({SchemaDiscovery.QuoteSqlServerIdentifier(exactSourceColumn!.Name)}, {SchemaDiscovery.QuoteSqlServerIdentifier(compatibleSourceColumn.Name)})",
                    targetColumnName,
                    $"{exactSourceColumn.Name} ?? {compatibleSourceColumn.Name}",
                    [exactSourceColumn.Name, compatibleSourceColumn.Name],
                    isConventionAlias
                        ? null
                        : $"Using legacy SQL Server source column fallback {compatibleSourceColumn.Name} -> {targetColumnName}.");
                return true;
            }

            sourceColumn = new ColumnValueMapping(
                SchemaDiscovery.QuoteSqlServerIdentifier(compatibleSourceColumn.Name),
                targetColumnName,
                compatibleSourceColumn.Name,
                [compatibleSourceColumn.Name],
                isConventionAlias ? null : compatibilityAlias.Reason);
            return true;
        }

        if (exactSourceColumnExists)
        {
            sourceColumn = new ColumnValueMapping(
                SchemaDiscovery.QuoteSqlServerIdentifier(exactSourceColumn!.Name),
                targetColumnName,
                exactSourceColumn.Name,
                [exactSourceColumn.Name],
                null);
            return true;
        }

        sourceColumn = new ColumnValueMapping(string.Empty, targetColumnName, string.Empty, [], null);
        return false;
    }

    private static bool IsLegacyForeignKeyConventionReason(string reason)
    {
        return reason.StartsWith("Using legacy SQL Server foreign-key naming convention", StringComparison.OrdinalIgnoreCase);
    }

    private static List<TableRef> GetCandidateTables(IEnumerable<TableRef> tables)
    {
        return tables
            .Where(table => !ShouldSkipTable(table))
            .OrderBy(table => table.Schema)
            .ThenBy(table => table.Name)
            .ToList();
    }

    private static void RenderIssues(string title, IEnumerable<string> issues)
    {
        CliConsole.RenderHints(title, issues.Take(20));
    }

    private static bool ShouldSkipTable(TableRef table)
    {
        if (LegacySqlServerSourceCompatibility.ShouldSkipTable(table))
        {
            return true;
        }

        if (table.Name.StartsWith("Hangfire", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (table.Name.Contains("ReadModel", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (table.Name.Equals("__EFMigrationsHistory", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (table.Name.Equals("__kitos_migration_journal", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private sealed record SchemaCompatibilityAssessment(
        IReadOnlyList<string> Errors,
        IReadOnlyList<string> Warnings);

    private sealed record ColumnValueMapping(
        string SelectExpression,
        string TargetColumnName,
        string DisplayName,
        IReadOnlyList<string> UsedSourceColumns,
        string? CompatibilityReason);

    private sealed record ColumnCompatibilityPlan(
        IReadOnlyList<string> InsertSourceSelectExpressions,
        IReadOnlyList<string> InsertTargetColumnNames,
        IReadOnlyList<ColumnValueMapping> DeferredSelfReferenceMappings,
        IReadOnlyList<string> Errors,
        IReadOnlyList<string> Warnings);

    private sealed record DeferredSelfReferenceUpdatePlan(
        IReadOnlyList<ColumnValueMapping> DeferredColumns,
        IReadOnlyList<ColumnValueMapping> PrimaryKeyColumns,
        IReadOnlyList<string> PrimaryKeyTargetColumnNames);

    private sealed record ColumnCopyPlan(
        IReadOnlyList<string> InsertSourceSelectExpressions,
        IReadOnlyList<string> InsertTargetColumnNames,
        DeferredSelfReferenceUpdatePlan? DeferredSelfReferenceUpdate);
}
