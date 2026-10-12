// INpgsqlSingletonOptions and NpgsqlAnnotationNames are internal Npgsql EF Core APIs required
// by the NpgsqlMigrationsSqlGenerator base class and for setting identity strategy annotations.
// There is no public alternative; suppress EF1001 for this file.
#pragma warning disable EF1001
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;

namespace Infrastructure.DataAccess;

/// <summary>
/// Overrides the default Npgsql migrations SQL generator to automatically qualify
/// all table/column/index operations with the "dbo" schema when no schema is explicitly
/// specified. This allows EF Core migration Up/Down methods to use standard constructs
/// (AddColumn, DropColumn, etc.) without needing schema-aware raw SQL.
///
/// The "public" schema is intentionally left untouched so that PostgreSQL extensions
/// (e.g. citext) that live in "public" are not affected.
/// </summary>
public sealed class KitosNpgsqlMigrationsSqlGenerator(
    MigrationsSqlGeneratorDependencies dependencies,
    INpgsqlSingletonOptions npgsqlSingletonOptions)
    : NpgsqlMigrationsSqlGenerator(dependencies, npgsqlSingletonOptions)
{
    private const string DefaultSchema = "dbo";
    private const string SqlServerIdentityAnnotationName = "SqlServer:Identity";
    private static readonly HashSet<string> DuplicateIndexNames = new(System.StringComparer.Ordinal)
    {
        "UX_Option_Uuid",
        "IX_Name",
        "IX_OrganizationId",
        "UX_NameUniqueToOrg",
        "UX_AccessModifier",
        "IX_Concluded",
        "UX_ExternalUuid",
        "IX_ExpirationDate",
        "IX_UserFullName",
        "IX_UserId",
        "IX_ProcurementInitiated",
        "IX_RoleId"
    };

    public override IReadOnlyList<MigrationCommand> Generate(
        IReadOnlyList<MigrationOperation> operations,
        IModel? model = null,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
    {
        var patched = operations.Select(PatchSchema).ToList();
        return base.Generate(patched, model, options);
    }

    private static MigrationOperation PatchSchema(MigrationOperation operation)
    {
        switch (operation)
        {
            case AddColumnOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                PatchColumn(op);
                break;
            case DropColumnOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                break;
            case AlterColumnOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                PatchColumn(op);
                break;
            case CreateTableOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                foreach (var column in op.Columns)
                {
                    PatchColumn(column);
                }
                if (op.PrimaryKey is not null)
                {
                    op.PrimaryKey.Name = GetPatchedConstraintName(op.PrimaryKey.Name, op.Name);
                }
                foreach (var uniqueConstraint in op.UniqueConstraints)
                {
                    uniqueConstraint.Name = GetPatchedConstraintName(uniqueConstraint.Name, op.Name);
                }
                foreach (var checkConstraint in op.CheckConstraints)
                {
                    checkConstraint.Name = GetPatchedConstraintName(checkConstraint.Name, op.Name);
                }
                foreach (var fk in op.ForeignKeys)
                {
                    fk.Name = GetPatchedConstraintName(fk.Name, op.Name);
                    if (fk.Schema is null) fk.Schema = DefaultSchema;
                    if (fk.PrincipalSchema is null) fk.PrincipalSchema = DefaultSchema;
                }
                break;
            case DropTableOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                break;
            case RenameTableOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                break;
            case AddForeignKeyOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                op.Name = GetPatchedConstraintName(op.Name, op.Table);
                if (op.PrincipalSchema is null) op.PrincipalSchema = DefaultSchema;
                break;
            case DropForeignKeyOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                op.Name = GetPatchedConstraintName(op.Name, op.Table);
                break;
            case AddPrimaryKeyOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                op.Name = GetPatchedConstraintName(op.Name, op.Table);
                break;
            case DropPrimaryKeyOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                op.Name = GetPatchedConstraintName(op.Name, op.Table);
                break;
            case AddUniqueConstraintOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                op.Name = GetPatchedConstraintName(op.Name, op.Table);
                break;
            case DropUniqueConstraintOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                op.Name = GetPatchedConstraintName(op.Name, op.Table);
                break;
            case AddCheckConstraintOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                op.Name = GetPatchedConstraintName(op.Name, op.Table);
                break;
            case DropCheckConstraintOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                op.Name = GetPatchedConstraintName(op.Name, op.Table);
                break;
            case CreateIndexOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                PatchIndexName(op);
                PatchIndexFilter(op);
                break;
            case DropIndexOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                PatchIndexName(op);
                break;
            case RenameColumnOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                break;
            case RenameIndexOperation op when op.Schema is null:
                op.Schema = DefaultSchema;
                PatchIndexName(op);
                break;
        }

        return operation;
    }

    private static void PatchColumn(ColumnOperation column)
    {
        column.ColumnType = column.ColumnType switch
        {
            "nvarchar(max)" => "text",
            "datetime2" => "timestamp without time zone",
            "bit" => "boolean",
            "uniqueidentifier" => "uuid",
            var type when type is not null && type.StartsWith("nvarchar(", System.StringComparison.OrdinalIgnoreCase)
                => type.Replace("nvarchar", "character varying", System.StringComparison.OrdinalIgnoreCase),
            _ => column.ColumnType
        };

        if (column[SqlServerIdentityAnnotationName] is not null)
        {
            column.RemoveAnnotation(SqlServerIdentityAnnotationName);
            column[NpgsqlAnnotationNames.ValueGenerationStrategy] = NpgsqlValueGenerationStrategy.IdentityByDefaultColumn;
        }
    }

    private static void PatchIndexName(CreateIndexOperation operation)
    {
        operation.Name = GetPatchedIndexName(operation.Name, operation.Table);
    }

    private static void PatchIndexFilter(CreateIndexOperation operation)
    {
        if (string.IsNullOrWhiteSpace(operation.Filter))
        {
            return;
        }

        operation.Filter = System.Text.RegularExpressions.Regex.Replace(
            operation.Filter,
            @"\[([^\]]+)\]",
            "\"$1\"",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant);
    }

    private static void PatchIndexName(DropIndexOperation operation)
    {
        operation.Name = GetPatchedIndexName(operation.Name, operation.Table);
    }

    private static void PatchIndexName(RenameIndexOperation operation)
    {
        operation.Name = GetPatchedIndexName(operation.Name, operation.Table);
        operation.NewName = GetPatchedIndexName(operation.NewName, operation.Table);
    }

    private static string GetPatchedIndexName(string indexName, string? tableName)
    {
        if (string.IsNullOrWhiteSpace(indexName))
        {
            return indexName;
        }

        var indexNameWithTableSuffix = !string.IsNullOrWhiteSpace(tableName) && DuplicateIndexNames.Contains(indexName)
            ? $"{indexName}_{tableName}"
            : indexName;

        return ToPostgreSqlIdentifier(indexNameWithTableSuffix, tableName);
    }

    private static string ToPostgreSqlIdentifier(string candidate, string? tableName)
    {
        const int maxIdentifierLength = 63;
        if (candidate.Length <= maxIdentifierLength)
        {
            return candidate;
        }

        const int hashLength = 8;
        var hashInput = string.IsNullOrWhiteSpace(tableName) ? candidate : $"{candidate}|{tableName}";
        var hash = System.Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hashInput)))[..hashLength];
        var prefixLength = maxIdentifierLength - hashLength - 1;
        return $"{candidate[..prefixLength]}_{hash}";
    }

    private static string GetPatchedConstraintName(string constraintName, string? tableName)
    {
        if (string.IsNullOrWhiteSpace(constraintName))
        {
            return constraintName;
        }

        return ToPostgreSqlIdentifier(constraintName, tableName);
    }
}
