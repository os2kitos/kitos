using System.Data;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace Tools.MigrateSQLServer2Postgres.Migration;

internal sealed record TableRef(string Schema, string Name)
{
    public override string ToString()
    {
        return $"{Schema}.{Name}";
    }
}

internal sealed record TableMatch(TableRef Source, TableRef Target);
internal sealed record TableMatchingResult(
    IReadOnlyList<TableMatch> Matches,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings);
internal sealed record SqlServerColumnDefinition(
    string Name,
    string DataType,
    int? MaxLength,
    int? Precision,
    int? Scale,
    bool IsNullable,
    bool IsIdentity,
    bool IsComputed,
    int Ordinal);
internal sealed record PostgresColumnDefinition(
    string Name,
    bool IsNullable,
    bool HasDefaultValue,
    bool IsIdentity,
    bool IsGenerated,
    int Ordinal);
internal sealed record PostgresForeignKeyDefinition(
    string Name,
    TableRef ChildTable,
    TableRef ParentTable,
    IReadOnlyList<string> ChildColumns,
    IReadOnlyList<string> ParentColumns);
internal sealed record SqlServerTableDefinition(TableRef Table, IReadOnlyList<SqlServerColumnDefinition> Columns);

internal sealed record PostgresUniqueIndexDefinition(TableRef Table, string IndexName, string IndexDdl);

internal sealed record PostgresUniqueConstraintDefinition(TableRef Table, string ConstraintName, IReadOnlyList<string> QuotedColumns);

internal static class SchemaDiscovery
{
    public static async Task<List<TableRef>> GetSqlServerTablesAsync(SqlConnection connection)
    {
        const string query = @"
SELECT s.name, t.name
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name;";

        var result = new List<TableRef>();
        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new TableRef(reader.GetString(0), reader.GetString(1)));
        }

        return result;
    }

    public static async Task<List<TableRef>> GetPostgresTablesAsync(NpgsqlConnection connection)
    {
        const string query = @"
SELECT table_schema, table_name
FROM information_schema.tables
WHERE table_type = 'BASE TABLE'
  AND table_schema NOT IN ('pg_catalog', 'information_schema')
ORDER BY table_schema, table_name;";

        var result = new List<TableRef>();
        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new TableRef(reader.GetString(0), reader.GetString(1)));
        }

        return result;
    }

    public static async Task<List<SqlServerTableDefinition>> GetSqlServerTableDefinitionsAsync(SqlConnection connection)
    {
        const string query = @"
SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    c.column_id AS Ordinal,
    c.name AS ColumnName,
    ty.name AS DataType,
    c.max_length AS MaxLength,
    c.precision AS Precision,
    c.scale AS Scale,
    c.is_nullable AS IsNullable,
    c.is_identity AS IsIdentity,
    c.is_computed AS IsComputed
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
INNER JOIN sys.columns c ON c.object_id = t.object_id
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name, c.column_id;";

        var rows = new List<(TableRef Table, SqlServerColumnDefinition Column)>();

        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var table = new TableRef(reader.GetString(0), reader.GetString(1));
            var column = new SqlServerColumnDefinition(
                reader.GetString(3),
                reader.GetString(4),
                reader.IsDBNull(5) ? null : reader.GetInt16(5),
                reader.IsDBNull(6) ? null : reader.GetByte(6),
                reader.IsDBNull(7) ? null : reader.GetByte(7),
                reader.GetBoolean(8),
                reader.GetBoolean(9),
                reader.GetBoolean(10),
                reader.GetInt32(2));

            rows.Add((table, column));
        }

        var definitions = rows
            .GroupBy(row => row.Table)
            .Select(group => new SqlServerTableDefinition(
                group.Key,
                group.Select(item => item.Column)
                    .OrderBy(column => column.Ordinal)
                    .ToList()))
            .OrderBy(definition => definition.Table.Schema)
            .ThenBy(definition => definition.Table.Name)
            .ToList();

        return definitions;
    }

    public static async Task<List<string>> GetSqlServerColumnsAsync(SqlConnection connection, TableRef table)
    {
        const string query = @"
SELECT c.name
FROM sys.columns c
INNER JOIN sys.tables t ON t.object_id = c.object_id
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = @schemaName
  AND t.name = @tableName
ORDER BY c.column_id;";

        var result = new List<string>();
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@schemaName", table.Schema);
        command.Parameters.AddWithValue("@tableName", table.Name);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }

    public static async Task<List<SqlServerColumnDefinition>> GetSqlServerColumnDefinitionsAsync(SqlConnection connection, TableRef table)
    {
        const string query = @"
SELECT
    c.column_id AS Ordinal,
    c.name AS ColumnName,
    ty.name AS DataType,
    c.max_length AS MaxLength,
    c.precision AS Precision,
    c.scale AS Scale,
    c.is_nullable AS IsNullable,
    c.is_identity AS IsIdentity,
    c.is_computed AS IsComputed
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
INNER JOIN sys.columns c ON c.object_id = t.object_id
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
WHERE s.name = @schemaName
  AND t.name = @tableName
ORDER BY c.column_id;";

        var result = new List<SqlServerColumnDefinition>();
        await using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@schemaName", table.Schema);
        command.Parameters.AddWithValue("@tableName", table.Name);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new SqlServerColumnDefinition(
                reader.GetString(1),
                reader.GetString(2),
                reader.IsDBNull(3) ? null : reader.GetInt16(3),
                reader.IsDBNull(4) ? null : reader.GetByte(4),
                reader.IsDBNull(5) ? null : reader.GetByte(5),
                reader.GetBoolean(6),
                reader.GetBoolean(7),
                reader.GetBoolean(8),
                reader.GetInt32(0)));
        }

        return result;
    }

    public static async Task<List<string>> GetPostgresColumnsAsync(NpgsqlConnection connection, TableRef table)
    {
        const string query = @"
SELECT column_name
FROM information_schema.columns
WHERE table_schema = @schemaName
  AND table_name = @tableName
ORDER BY ordinal_position;";

        var result = new List<string>();
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@schemaName", table.Schema);
        command.Parameters.AddWithValue("@tableName", table.Name);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }

    public static async Task<List<PostgresColumnDefinition>> GetPostgresColumnDefinitionsAsync(NpgsqlConnection connection, TableRef table)
    {
        const string query = @"
SELECT
    column_name,
    is_nullable,
    column_default,
    is_identity,
    is_generated,
    ordinal_position
FROM information_schema.columns
WHERE table_schema = @schemaName
  AND table_name = @tableName
ORDER BY ordinal_position;";

        var result = new List<PostgresColumnDefinition>();
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@schemaName", table.Schema);
        command.Parameters.AddWithValue("@tableName", table.Name);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var isNullable = string.Equals(reader.GetString(1), "YES", StringComparison.OrdinalIgnoreCase);
            var hasDefaultValue = !reader.IsDBNull(2) && !string.IsNullOrWhiteSpace(reader.GetString(2));
            var isIdentity = !reader.IsDBNull(3) && string.Equals(reader.GetString(3), "YES", StringComparison.OrdinalIgnoreCase);
            var isGenerated = !reader.IsDBNull(4) && !string.Equals(reader.GetString(4), "NEVER", StringComparison.OrdinalIgnoreCase);

            result.Add(new PostgresColumnDefinition(
                reader.GetString(0),
                isNullable,
                hasDefaultValue,
                isIdentity,
                isGenerated,
                reader.GetInt32(5)));
        }

        return result;
    }

    public static async Task<List<(TableRef Child, TableRef Parent)>> GetSqlServerForeignKeyEdgesAsync(SqlConnection connection)
    {
        const string query = @"
SELECT childSchema.name, childTable.name, parentSchema.name, parentTable.name
FROM sys.foreign_keys fk
INNER JOIN sys.tables childTable ON fk.parent_object_id = childTable.object_id
INNER JOIN sys.schemas childSchema ON childTable.schema_id = childSchema.schema_id
INNER JOIN sys.tables parentTable ON fk.referenced_object_id = parentTable.object_id
INNER JOIN sys.schemas parentSchema ON parentTable.schema_id = parentSchema.schema_id
WHERE childTable.is_ms_shipped = 0 AND parentTable.is_ms_shipped = 0;";

        var result = new List<(TableRef Child, TableRef Parent)>();
        await using var command = new SqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var child = new TableRef(reader.GetString(0), reader.GetString(1));
            var parent = new TableRef(reader.GetString(2), reader.GetString(3));
            result.Add((child, parent));
        }

        return result;
    }

    public static async Task<List<PostgresForeignKeyDefinition>> GetPostgresForeignKeysAsync(NpgsqlConnection connection)
    {
        const string query = @"
SELECT
    constraint_name,
    child_schema,
    child_table,
    parent_schema,
    parent_table,
    child_columns,
    parent_columns
FROM (
    SELECT
        c.conname AS constraint_name,
        child_ns.nspname AS child_schema,
        child_table.relname AS child_table,
        parent_ns.nspname AS parent_schema,
        parent_table.relname AS parent_table,
        ARRAY(
            SELECT child_att.attname
            FROM unnest(c.conkey) WITH ORDINALITY AS child_key(attnum, ordinal)
            INNER JOIN pg_attribute child_att
                ON child_att.attrelid = c.conrelid
               AND child_att.attnum = child_key.attnum
            ORDER BY child_key.ordinal
        ) AS child_columns,
        ARRAY(
            SELECT parent_att.attname
            FROM unnest(c.confkey) WITH ORDINALITY AS parent_key(attnum, ordinal)
            INNER JOIN pg_attribute parent_att
                ON parent_att.attrelid = c.confrelid
               AND parent_att.attnum = parent_key.attnum
            ORDER BY parent_key.ordinal
        ) AS parent_columns
    FROM pg_constraint c
    INNER JOIN pg_class child_table ON child_table.oid = c.conrelid
    INNER JOIN pg_namespace child_ns ON child_ns.oid = child_table.relnamespace
    INNER JOIN pg_class parent_table ON parent_table.oid = c.confrelid
    INNER JOIN pg_namespace parent_ns ON parent_ns.oid = parent_table.relnamespace
    WHERE c.contype = 'f'
) foreign_keys
ORDER BY child_schema, child_table, constraint_name;";

        var result = new List<PostgresForeignKeyDefinition>();
        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new PostgresForeignKeyDefinition(
                reader.GetString(0),
                new TableRef(reader.GetString(1), reader.GetString(2)),
                new TableRef(reader.GetString(3), reader.GetString(4)),
                reader.GetFieldValue<string[]>(5),
                reader.GetFieldValue<string[]>(6)));
        }

        return result;
    }

    public static async Task<List<string>> GetPostgresPrimaryKeyColumnsAsync(NpgsqlConnection connection, TableRef table)
    {
        const string query = @"
SELECT kcu.column_name
FROM information_schema.table_constraints tc
INNER JOIN information_schema.key_column_usage kcu
    ON kcu.constraint_schema = tc.constraint_schema
   AND kcu.constraint_name = tc.constraint_name
   AND kcu.table_schema = tc.table_schema
   AND kcu.table_name = tc.table_name
WHERE tc.constraint_type = 'PRIMARY KEY'
  AND tc.table_schema = @schemaName
  AND tc.table_name = @tableName
ORDER BY kcu.ordinal_position;";

        var result = new List<string>();
        await using var command = new NpgsqlCommand(query, connection);
        command.Parameters.AddWithValue("@schemaName", table.Schema);
        command.Parameters.AddWithValue("@tableName", table.Name);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }

    public static TableMatchingResult AnalyzeTableMatches(IReadOnlyCollection<TableRef> sourceTables, IReadOnlyCollection<TableRef> targetTables)
    {
        static string ToQualifiedKey(TableRef table)
        {
            return $"{table.Schema}\u001f{table.Name}";
        }

        var errors = new List<string>();
        var warnings = new List<string>();

        var targetByQualifiedName = targetTables
            .GroupBy(ToQualifiedKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        var targetByName = targetTables
            .GroupBy(table => table.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        foreach (var duplicateByName in targetByName
                     .Where(group => group.Value.Select(table => table.Schema).Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
                     .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
        {
            var schemas = duplicateByName.Value
                .Select(table => table.Schema)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(schema => schema, StringComparer.OrdinalIgnoreCase);

            warnings.Add($"Target contains duplicate table name '{duplicateByName.Key}' across schemas: {string.Join(", ", schemas)}. Exact schema matching will be enforced.");
        }

        var matches = new List<TableMatch>();
        foreach (var sourceTable in sourceTables.OrderBy(table => table.Schema).ThenBy(table => table.Name))
        {
            if (targetByQualifiedName.TryGetValue(ToQualifiedKey(sourceTable), out var exactMatches))
            {
                matches.Add(new TableMatch(sourceTable, exactMatches[0]));
                continue;
            }

            var compatibilityMatches = LegacySqlServerSourceCompatibility.GetCompatibleTargetTables(sourceTable)
                .Where(candidate => targetByQualifiedName.ContainsKey(ToQualifiedKey(candidate.TargetTable)))
                .ToList();

            if (compatibilityMatches.Count == 1)
            {
                var compatibilityMatch = compatibilityMatches[0];
                matches.Add(new TableMatch(sourceTable, compatibilityMatch.TargetTable));
                warnings.Add(compatibilityMatch.Reason);
                continue;
            }

            if (compatibilityMatches.Count > 1)
            {
                errors.Add($"Source table {sourceTable} resolves to multiple PostgreSQL target tables through legacy compatibility aliases: {string.Join(", ", compatibilityMatches.Select(candidate => candidate.TargetTable.ToString()).OrderBy(name => name, StringComparer.OrdinalIgnoreCase))}.");
                continue;
            }

            if (targetByName.TryGetValue(sourceTable.Name, out var candidates))
            {
                errors.Add($"Source table {sourceTable} is missing a compatible target schema match. Found same table name in target schema(s): {string.Join(", ", candidates.Select(candidate => candidate.Schema).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(schema => schema, StringComparer.OrdinalIgnoreCase))}.");
                continue;
            }

            errors.Add($"Source table {sourceTable} does not map to a table in the PostgreSQL target.");
        }

        foreach (var duplicateTarget in matches
                     .GroupBy(match => ToQualifiedKey(match.Target), StringComparer.OrdinalIgnoreCase)
                     .Where(group => group.Count() > 1)
                     .OrderBy(group => group.Key, StringComparer.OrdinalIgnoreCase))
        {
            errors.Add($"Multiple SQL Server source tables resolve to the same PostgreSQL target table {duplicateTarget.First().Target}: {string.Join(", ", duplicateTarget.Select(match => match.Source.ToString()).OrderBy(name => name, StringComparer.OrdinalIgnoreCase))}.");
        }

        return new TableMatchingResult(matches, errors, warnings);
    }

    public static List<TableRef> TopologicalSortTables(
        IReadOnlyCollection<TableRef> tables,
        IReadOnlyCollection<(TableRef Child, TableRef Parent)> foreignKeyEdges)
    {
        var tableSet = new HashSet<TableRef>(tables);
        var inDegree = tables.ToDictionary(table => table, _ => 0);
        var adjacency = tables.ToDictionary(table => table, _ => new List<TableRef>());

        foreach (var edge in foreignKeyEdges)
        {
            if (!tableSet.Contains(edge.Child) || !tableSet.Contains(edge.Parent))
            {
                continue;
            }

            if (edge.Child == edge.Parent)
            {
                continue;
            }

            adjacency[edge.Parent].Add(edge.Child);
            inDegree[edge.Child]++;
        }

        var queue = new Queue<TableRef>(inDegree.Where(pair => pair.Value == 0)
            .Select(pair => pair.Key)
            .OrderBy(table => table.Schema)
            .ThenBy(table => table.Name));

        var result = new List<TableRef>(tables.Count);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);

            foreach (var dependent in adjacency[current])
            {
                inDegree[dependent]--;
                if (inDegree[dependent] == 0)
                {
                    queue.Enqueue(dependent);
                }
            }
        }

        if (result.Count == tables.Count)
        {
            return result;
        }

        // Cycles remain. Order the remaining tables by how many other remaining members depend on
        // them (descending): hub tables like Organization that many others FK into should come
        // first. Break ties alphabetically. This is a greedy heuristic — it cannot guarantee a
        // perfect ordering across all cycles, but it produces a far better result than simple
        // alphabetical fallback for the typical KITOS FK graph.
        var cycleMembers = new HashSet<TableRef>(tables.Except(result));
        var cycleOrdered = cycleMembers
            .OrderByDescending(table => adjacency[table].Count(dep => cycleMembers.Contains(dep)))
            .ThenBy(table => table.Schema)
            .ThenBy(table => table.Name)
            .ToList();
        result.AddRange(cycleOrdered);
        return result;
    }

    public static string QuoteSqlServerIdentifier(string identifier)
    {
        return $"[{identifier.Replace("]", "]]", StringComparison.Ordinal)}]";
    }

    public static string QuotePostgresIdentifier(string identifier)
    {
        return $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }

    public static string QualifySqlServerTable(TableRef table)
    {
        return $"{QuoteSqlServerIdentifier(table.Schema)}.{QuoteSqlServerIdentifier(table.Name)}";
    }

    public static string QualifyPostgresTable(TableRef table)
    {
        return $"{QuotePostgresIdentifier(table.Schema)}.{QuotePostgresIdentifier(table.Name)}";
    }

    /// <summary>
    /// Returns all non-primary-key unique indexes on the given tables, including their full DDL
    /// (via pg_get_indexdef) so they can be dropped and recreated verbatim.
    /// </summary>
    public static async Task<List<PostgresUniqueIndexDefinition>> GetPostgresUniqueIndexesAsync(
        NpgsqlConnection connection,
        IReadOnlyCollection<TableRef> tables)
    {
        // Build a temporary table list as a VALUES clause so we can filter to only the tables
        // we are migrating, without needing a temporary table or multiple round-trips.
        if (tables.Count == 0)
        {
            return [];
        }

        const string query = @"
SELECT
    n.nspname                  AS schema_name,
    t.relname                  AS table_name,
    i.relname                  AS index_name,
    pg_get_indexdef(ix.indexrelid) AS index_ddl
FROM pg_index ix
INNER JOIN pg_class t  ON t.oid  = ix.indrelid
INNER JOIN pg_class i  ON i.oid  = ix.indexrelid
INNER JOIN pg_namespace n ON n.oid = t.relnamespace
WHERE ix.indisunique = true
  AND ix.indisprimary = false
  AND NOT EXISTS (
      -- Exclude indexes that back unique constraints (they share the same name via pg_constraint)
      -- We drop the index directly, so constraint-backed unique indexes must be excluded;
      -- their constraint is the real owner and dropping the index would fail.
      -- Instead we handle those via ALTER TABLE DROP CONSTRAINT / ADD CONSTRAINT.
      SELECT 1 FROM pg_constraint c
      WHERE c.conindid = ix.indexrelid
        AND c.contype = 'u'
  )
ORDER BY n.nspname, t.relname, i.relname;";

        var result = new List<PostgresUniqueIndexDefinition>();
        var tableSet = tables
            .Select(t => (t.Schema.ToLowerInvariant(), t.Name.ToLowerInvariant()))
            .ToHashSet();

        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var schema = reader.GetString(0);
            var table = reader.GetString(1);
            if (!tableSet.Contains((schema.ToLowerInvariant(), table.ToLowerInvariant())))
            {
                continue;
            }

            result.Add(new PostgresUniqueIndexDefinition(
                new TableRef(schema, table),
                reader.GetString(2),
                reader.GetString(3)));
        }

        return result;
    }

    /// <summary>
    /// Returns all unique constraints (backed by a unique index via pg_constraint) on the given tables.
    /// These must be dropped via ALTER TABLE DROP CONSTRAINT and restored via ALTER TABLE ADD CONSTRAINT UNIQUE.
    /// </summary>
    public static async Task<List<PostgresUniqueConstraintDefinition>> GetPostgresUniqueConstraintsAsync(
        NpgsqlConnection connection,
        IReadOnlyCollection<TableRef> tables)
    {
        if (tables.Count == 0)
        {
            return [];
        }

        const string query = @"
SELECT
    n.nspname                          AS schema_name,
    t.relname                          AS table_name,
    c.conname                          AS constraint_name,
    string_agg(a.attname, ',' ORDER BY x.ordinality) AS columns
FROM pg_constraint c
INNER JOIN pg_class t       ON t.oid = c.conrelid
INNER JOIN pg_namespace n   ON n.oid = t.relnamespace
CROSS JOIN LATERAL unnest(c.conkey) WITH ORDINALITY AS x(attnum, ordinality)
INNER JOIN pg_attribute a   ON a.attrelid = t.oid AND a.attnum = x.attnum
WHERE c.contype = 'u'
GROUP BY n.nspname, t.relname, c.conname
ORDER BY n.nspname, t.relname, c.conname;";

        var result = new List<PostgresUniqueConstraintDefinition>();
        var tableSet = tables
            .Select(t => (t.Schema.ToLowerInvariant(), t.Name.ToLowerInvariant()))
            .ToHashSet();

        await using var command = new NpgsqlCommand(query, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var schema = reader.GetString(0);
            var table = reader.GetString(1);
            if (!tableSet.Contains((schema.ToLowerInvariant(), table.ToLowerInvariant())))
            {
                continue;
            }

            var columns = reader.GetString(3)
                .Split(',')
                .Select(QuotePostgresIdentifier)
                .ToList();

            result.Add(new PostgresUniqueConstraintDefinition(
                new TableRef(schema, table),
                reader.GetString(2),
                columns));
        }

        return result;
    }
}
