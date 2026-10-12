using Npgsql;

namespace Tools.MigrateSQLServer2Postgres.Migration;

internal enum JournalStatus
{
    Started,
    Succeeded,
    Failed
}

internal static class MigrationJournal
{
    private const string JournalSchema = "public";
    private const string JournalTable = "__kitos_migration_journal";

    public static async Task EnsureJournalTableAsync(NpgsqlConnection targetConnection)
    {
        var sql = $@"
CREATE TABLE IF NOT EXISTS {SchemaDiscovery.QuotePostgresIdentifier(JournalSchema)}.{SchemaDiscovery.QuotePostgresIdentifier(JournalTable)}
(
    table_schema text NOT NULL,
    table_name text NOT NULL,
    status text NOT NULL,
    started_at_utc timestamptz NOT NULL,
    finished_at_utc timestamptz NULL,
    rows_copied bigint NULL,
    error text NULL,
    PRIMARY KEY (table_schema, table_name)
);";

        await using var command = new NpgsqlCommand(sql, targetConnection);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task<bool> WasSuccessfulAsync(NpgsqlConnection targetConnection, TableRef table)
    {
        var sql = $@"
SELECT status
FROM {SchemaDiscovery.QuotePostgresIdentifier(JournalSchema)}.{SchemaDiscovery.QuotePostgresIdentifier(JournalTable)}
WHERE table_schema = @schemaName AND table_name = @tableName;";

        await using var command = new NpgsqlCommand(sql, targetConnection);
        command.Parameters.AddWithValue("@schemaName", table.Schema);
        command.Parameters.AddWithValue("@tableName", table.Name);

        var status = await command.ExecuteScalarAsync();
        return status is string stringStatus && stringStatus.Equals(JournalStatus.Succeeded.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public static Task MarkStartedAsync(NpgsqlConnection targetConnection, TableRef table)
    {
        return UpsertAsync(targetConnection, table, JournalStatus.Started, null, null);
    }

    public static Task MarkSucceededAsync(NpgsqlConnection targetConnection, TableRef table, long rowsCopied)
    {
        return UpsertAsync(targetConnection, table, JournalStatus.Succeeded, rowsCopied, null);
    }

    public static Task MarkFailedAsync(NpgsqlConnection targetConnection, TableRef table, string error)
    {
        return UpsertAsync(targetConnection, table, JournalStatus.Failed, null, error);
    }

    private static async Task UpsertAsync(NpgsqlConnection targetConnection, TableRef table, JournalStatus status, long? rowsCopied, string? error)
    {
        var sql = $@"
INSERT INTO {SchemaDiscovery.QuotePostgresIdentifier(JournalSchema)}.{SchemaDiscovery.QuotePostgresIdentifier(JournalTable)}
(
    table_schema,
    table_name,
    status,
    started_at_utc,
    finished_at_utc,
    rows_copied,
    error
)
VALUES
(
    @schemaName,
    @tableName,
    @status,
    CASE WHEN @status = @startedStatus THEN NOW() ELSE COALESCE((SELECT started_at_utc FROM {SchemaDiscovery.QuotePostgresIdentifier(JournalSchema)}.{SchemaDiscovery.QuotePostgresIdentifier(JournalTable)} WHERE table_schema = @schemaName AND table_name = @tableName), NOW()) END,
    CASE WHEN @status IN (@succeededStatus, @failedStatus) THEN NOW() ELSE NULL END,
    @rowsCopied,
    @error
)
ON CONFLICT (table_schema, table_name)
DO UPDATE SET
    status = EXCLUDED.status,
    finished_at_utc = EXCLUDED.finished_at_utc,
    rows_copied = EXCLUDED.rows_copied,
    error = EXCLUDED.error,
    started_at_utc = CASE WHEN EXCLUDED.status = @startedStatus THEN NOW() ELSE {SchemaDiscovery.QuotePostgresIdentifier(JournalTable)}.started_at_utc END;";

        await using var command = new NpgsqlCommand(sql, targetConnection);
        command.Parameters.AddWithValue("@schemaName", table.Schema);
        command.Parameters.AddWithValue("@tableName", table.Name);
        command.Parameters.AddWithValue("@status", status.ToString());
        command.Parameters.AddWithValue("@startedStatus", JournalStatus.Started.ToString());
        command.Parameters.AddWithValue("@succeededStatus", JournalStatus.Succeeded.ToString());
        command.Parameters.AddWithValue("@failedStatus", JournalStatus.Failed.ToString());
        command.Parameters.AddWithValue("@rowsCopied", (object?)rowsCopied ?? DBNull.Value);
        command.Parameters.AddWithValue("@error", (object?)error ?? DBNull.Value);
        await command.ExecuteNonQueryAsync();
    }
}
