using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class MakeSystemUsageCriticalityChoiceType_InsteadOfGdprCriticalityEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {            migrationBuilder.DropIndex(
                name: "ItSystemUsageOverviewReadModel_Index_GdprCriticality",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "GdprCriticality",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.RenameColumn(
                name: "GdprCriticality",
                table: "ItSystemUsage",
                newName: "SystemUsageCriticalityLevelId");

            migrationBuilder.RenameIndex(
                name: "ItSystemUsage_Index_GdprCriticality",
                table: "ItSystemUsage",
                newName: "IX_ItSystemUsage_SystemUsageCriticalityLevelId");

            migrationBuilder.AddColumn<string>(
                name: "CriticalityLevelDocumentationUrl",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CriticalityLevelDocumentationUrlName",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SystemUsageCriticalityLevelUuid",
                table: "ItSystemUsageOverviewReadModels",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CriticalityLevelDocumentationName",
                table: "ItSystemUsage",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CriticalityLevelDocumentationUrl",
                table: "ItSystemUsage",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LocalSystemUsageCriticalityLevelTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    OptionId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalSystemUsageCriticalityLevelTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalSystemUsageCriticalityLevelTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalSystemUsageCriticalityLevelTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalSystemUsageCriticalityLevelTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SystemUsageCriticalityLevelTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemUsageCriticalityLevelTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemUsageCriticalityLevelTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemUsageCriticalityLevelTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_SystemUsageCriticalityLevelUuid",
                table: "ItSystemUsageOverviewReadModels",
                column: "SystemUsageCriticalityLevelUuid");

            migrationBuilder.CreateIndex(
                name: "IX_LocalSystemUsageCriticalityLevelTypes_LastChangedByUserId",
                table: "LocalSystemUsageCriticalityLevelTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalSystemUsageCriticalityLevelTypes_ObjectOwnerId",
                table: "LocalSystemUsageCriticalityLevelTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalSystemUsageCriticalityLevelTypes_OrganizationId",
                table: "LocalSystemUsageCriticalityLevelTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemUsageCriticalityLevelTypes_LastChangedByUserId",
                table: "SystemUsageCriticalityLevelTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemUsageCriticalityLevelTypes_ObjectOwnerId",
                table: "SystemUsageCriticalityLevelTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "SystemUsageCriticalityLevelTypes",
                column: "Uuid",
                unique: true);

            var seedAndRemapSql = @"
                DO $$
                BEGIN
                    -- If a global admin user exists, seed option types and remap old enum values.
                    -- On a fresh empty database (no users yet) the whole block is skipped;
                    -- the application-level option type seeder handles that case on first startup.
                    IF EXISTS (SELECT 1 FROM dbo.""User"" WHERE ""IsGlobalAdmin"" = TRUE) THEN
                        INSERT INTO dbo.""SystemUsageCriticalityLevelTypes""
                            (""ObjectOwnerId"", ""LastChanged"", ""LastChangedByUserId"", ""Name"", ""IsLocallyAvailable"", ""IsObligatory"", ""Description"", ""IsEnabled"", ""Priority"", ""Uuid"")
                        SELECT
                            (SELECT ""Id"" FROM dbo.""User"" WHERE ""IsGlobalAdmin"" = TRUE ORDER BY ""Id"" LIMIT 1),
                            NOW() AT TIME ZONE 'UTC',
                            (SELECT ""Id"" FROM dbo.""User"" WHERE ""IsGlobalAdmin"" = TRUE ORDER BY ""Id"" LIMIT 1),
                            v.""Name"", v.""IsLocallyAvailable"", v.""IsObligatory"", NULL, v.""IsEnabled"", v.""Priority"", md5(random()::text || clock_timestamp()::text || v.""Name"")::uuid
                        FROM (VALUES
                            ('Ikke kritisk', TRUE, FALSE, TRUE, 0),
                            ('Lav',          TRUE, FALSE, TRUE, 1),
                            ('Mellem',       TRUE, FALSE, TRUE, 2),
                            ('Høj',          TRUE, FALSE, TRUE, 3),
                            ('Meget høj',    TRUE, FALSE, TRUE, 4)
                        ) AS v(""Name"", ""IsLocallyAvailable"", ""IsObligatory"", ""IsEnabled"", ""Priority"");

                        -- Remap old enum integers to new option type FK IDs
                        UPDATE dbo.""ItSystemUsage"" AS isu
                        SET ""SystemUsageCriticalityLevelId"" = opt.""Id""
                        FROM dbo.""SystemUsageCriticalityLevelTypes"" AS opt
                        WHERE isu.""SystemUsageCriticalityLevelId"" IS NOT NULL
                          AND opt.""Name"" = CASE isu.""SystemUsageCriticalityLevelId""
                              WHEN 0 THEN 'Ikke kritisk'
                              WHEN 1 THEN 'Lav'
                              WHEN 2 THEN 'Mellem'
                              WHEN 3 THEN 'Høj'
                              WHEN 4 THEN 'Meget høj'
                          END;

                        -- Null out any rows whose value was not a recognised enum integer (defensive cleanup)
                        UPDATE dbo.""ItSystemUsage""
                        SET ""SystemUsageCriticalityLevelId"" = NULL
                        WHERE ""SystemUsageCriticalityLevelId"" IS NOT NULL
                          AND ""SystemUsageCriticalityLevelId"" NOT IN (SELECT ""Id"" FROM dbo.""SystemUsageCriticalityLevelTypes"");
                    END IF;
                END
                $$;";

            migrationBuilder.Sql(seedAndRemapSql);

            migrationBuilder.AddForeignKey(
                name: "FK_ItSystemUsage_SystemUsageCriticalityLevelTypes_SystemUsageCriticalityLevelId",
                table: "ItSystemUsage",
                column: "SystemUsageCriticalityLevelId",
                principalTable: "SystemUsageCriticalityLevelTypes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItSystemUsage_SystemUsageCriticalityLevelTypes_SystemUsageCriticalityLevelId",
                table: "ItSystemUsage");

            migrationBuilder.DropTable(
                name: "LocalSystemUsageCriticalityLevelTypes");

            migrationBuilder.DropTable(
                name: "SystemUsageCriticalityLevelTypes");

            migrationBuilder.DropIndex(
                name: "ItSystemUsageOverviewReadModel_Index_SystemUsageCriticalityLevelUuid",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "CriticalityLevelDocumentationUrl",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "CriticalityLevelDocumentationUrlName",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "SystemUsageCriticalityLevelUuid",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "CriticalityLevelDocumentationName",
                table: "ItSystemUsage");

            migrationBuilder.DropColumn(
                name: "CriticalityLevelDocumentationUrl",
                table: "ItSystemUsage");

            migrationBuilder.RenameColumn(
                name: "SystemUsageCriticalityLevelId",
                table: "ItSystemUsage",
                newName: "GdprCriticality");

            migrationBuilder.RenameIndex(
                name: "IX_ItSystemUsage_SystemUsageCriticalityLevelId",
                table: "ItSystemUsage",
                newName: "ItSystemUsage_Index_GdprCriticality");

            migrationBuilder.AddColumn<int>(
                name: "GdprCriticality",
                table: "ItSystemUsageOverviewReadModels",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_GdprCriticality",
                table: "ItSystemUsageOverviewReadModels",
                column: "GdprCriticality");
        }
    }
}
