namespace Tools.MigrateSQLServer2Postgres.Migration;

internal sealed record LegacyTargetTableAlias(TableRef TargetTable, string Reason);
internal sealed record LegacySourceColumnAlias(string SourceColumnName, string Reason);

internal static class LegacySqlServerSourceCompatibility
{
    private const char QualifiedNameSeparator = '\u001f';

    private static readonly IReadOnlyDictionary<string, TableRef> LegacySourceTableAliases =
        new Dictionary<string, TableRef>(StringComparer.OrdinalIgnoreCase)
        {
            [ToQualifiedKey("dbo", "LocalFrequencyTypes")] = new("dbo", "LocalRelationFrequencyTypes"),
            [ToQualifiedKey("dbo", "SsoOrganizationIdentities")] = new("dbo", "StsOrganizationIdentities"),
            [ToQualifiedKey("dbo", "Subscriptions")] = new("public", "Subscriptions")
        };

    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>> LegacySourceColumnAliases =
        new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<string>>>(StringComparer.OrdinalIgnoreCase)
        {
            [ToQualifiedKey("dbo", "OrganizationRights")] = CreateTargetColumnAliasMap(new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["OrganizationId"] = ["ObjectId"]
            }),
            [ToQualifiedKey("dbo", "ItSystem")] = CreateTargetColumnAliasMap(new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["TerminationDeadlineTypesInSystem_Id"] = ["TerminationDeadlineInSystem_Id"],
                ["LegalName"] = ["DBSName"],
                ["LegalDataProcessorName"] = ["DBSDataProcessorName"]
            }),
            [ToQualifiedKey("dbo", "SystemRelations")] = CreateTargetColumnAliasMap(new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["FromSystemUsageId"] = ["RelationSourceId"],
                ["ToSystemUsageId"] = ["RelationTargetId"]
            }),
            [ToQualifiedKey("dbo", "DataProcessingRegistrations")] = CreateTargetColumnAliasMap(new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["BasisForTransferId"] = ["DataProcessingBasisForTransferOption_Id"],
                ["DataResponsible_Id"] = ["DataProcessingDataResponsibleOption_Id"]
            }),
            [ToQualifiedKey("dbo", "ItContract")] = CreateTargetColumnAliasMap(new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["ProcurementPlanQuarter"] = ["ProcurementPlanHalf"],
                ["CriticalityId"] = ["CriticalityTypeId"]
            }),
            [ToQualifiedKey("dbo", "ItSystemUsage")] = CreateTargetColumnAliasMap(new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["ArchiveSupplierId"] = ["SupplierId"]
            })
        };

    public static bool ShouldSkipTable(TableRef table)
    {
        return table.Name.Equals("__MigrationHistory", StringComparison.OrdinalIgnoreCase)
               || table.Name.Equals("OrganizationOptions", StringComparison.OrdinalIgnoreCase);
    }

    public static IReadOnlyList<LegacyTargetTableAlias> GetCompatibleTargetTables(TableRef sourceTable)
    {
        if (LegacySourceTableAliases.TryGetValue(ToQualifiedKey(sourceTable), out var targetTable))
        {
            return
            [
                new LegacyTargetTableAlias(
                    targetTable,
                    $"Using legacy SQL Server source table alias {sourceTable} -> {targetTable}.")
            ];
        }

        return [];
    }

    public static IReadOnlyList<LegacySourceColumnAlias> GetCompatibleSourceColumns(TableRef targetTable, string targetColumnName)
    {
        var aliases = new List<LegacySourceColumnAlias>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (LegacySourceColumnAliases.TryGetValue(ToQualifiedKey(targetTable), out var targetColumnAliases)
            && targetColumnAliases.TryGetValue(targetColumnName, out var explicitAliases))
        {
            foreach (var explicitAlias in explicitAliases)
            {
                if (seen.Add(explicitAlias))
                {
                    aliases.Add(new LegacySourceColumnAlias(
                        explicitAlias,
                        $"Using explicit legacy SQL Server source column alias {explicitAlias} -> {targetColumnName}."));
                }
            }
        }

        if (TryGetLegacyForeignKeyAlias(targetColumnName, out var conventionAlias) && seen.Add(conventionAlias))
        {
            aliases.Add(new LegacySourceColumnAlias(
                conventionAlias,
                $"Using legacy SQL Server foreign-key naming convention {conventionAlias} -> {targetColumnName}."));
        }

        return aliases;
    }

    private static bool TryGetLegacyForeignKeyAlias(string targetColumnName, out string sourceColumnName)
    {
        if (targetColumnName.Length > 2
            && targetColumnName.EndsWith("Id", StringComparison.Ordinal)
            && !targetColumnName.EndsWith("_Id", StringComparison.Ordinal))
        {
            sourceColumnName = $"{targetColumnName[..^2]}_Id";
            return true;
        }

        sourceColumnName = string.Empty;
        return false;
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> CreateTargetColumnAliasMap(IDictionary<string, IReadOnlyList<string>> aliases)
    {
        return new Dictionary<string, IReadOnlyList<string>>(aliases, StringComparer.OrdinalIgnoreCase);
    }

    private static string ToQualifiedKey(TableRef table)
    {
        return ToQualifiedKey(table.Schema, table.Name);
    }

    private static string ToQualifiedKey(string schema, string name)
    {
        return $"{schema}{QualifiedNameSeparator}{name}";
    }
}
