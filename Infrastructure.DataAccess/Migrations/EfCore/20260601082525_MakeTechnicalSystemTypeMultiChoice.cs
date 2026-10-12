using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class MakeTechnicalSystemTypeMultiChoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {            // Create the many-to-many join table
            migrationBuilder.CreateTable(
                name: "ItSystemUsageTechnicalSystemTypes",
                columns: table => new
                {
                    ReferencesId = table.Column<int>(type: "integer", nullable: false),
                    TechnicalSystemTypesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageTechnicalSystemTypes", x => new { x.ReferencesId, x.TechnicalSystemTypesId });
                    table.ForeignKey(
                        name: "FK_ItSystemUsageTechnicalSystemTypes_ItSystemUsage_ReferencesId",
                        column: x => x.ReferencesId,
                        principalTable: "ItSystemUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageTechnicalSystemTypes_TechnicalSystemTypes_TechnicalSystemTypesId",
                        column: x => x.TechnicalSystemTypesId,
                        principalTable: "TechnicalSystemTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageTechnicalSystemTypes_TechnicalSystemTypesId",
                table: "ItSystemUsageTechnicalSystemTypes",
                column: "TechnicalSystemTypesId");

            // Migrate existing single-choice data to the new join table
            var migrateExistingDataSql = @"INSERT INTO dbo.""ItSystemUsageTechnicalSystemTypes"" (""ReferencesId"", ""TechnicalSystemTypesId"")
                    SELECT ""Id"", ""TechnicalSystemTypeId""
                    FROM dbo.""ItSystemUsage""
                    WHERE ""TechnicalSystemTypeId"" IS NOT NULL
                ";

            migrationBuilder.Sql(migrateExistingDataSql);

            // Drop the old FK and column from ItSystemUsage
            migrationBuilder.DropForeignKey(
                name: "FK_ItSystemUsage_TechnicalSystemTypes_TechnicalSystemTypeId",
                table: "ItSystemUsage");

            migrationBuilder.DropIndex(
                name: "IX_ItSystemUsage_TechnicalSystemTypeId",
                table: "ItSystemUsage");

            migrationBuilder.DropColumn(
                name: "TechnicalSystemTypeId",
                table: "ItSystemUsage");

            // Drop old read model columns
            migrationBuilder.DropIndex(
                name: "ItSystemUsageOverviewReadModel_Index_TechnicalSystemTypeUuid",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "TechnicalSystemTypeUuid",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "TechnicalSystemTypeName",
                table: "ItSystemUsageOverviewReadModels");

            // Add CSV column to read model
            migrationBuilder.AddColumn<string>(
                name: "TechnicalSystemTypeNamesAsCsv",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);

            // Create the new read model child table
            migrationBuilder.CreateTable(
                name: "ItSystemUsageOverviewTechnicalSystemTypeReadModel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TechnicalSystemTypeUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    TechnicalSystemTypeName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageOverviewTechnicalSystemTypeReadModel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOverviewTechnicalSystemTypeReadModel_ItSystemUsageOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItSystemUsageOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewTechnicalSystemTypeReadModel_Index_Uuid",
                table: "ItSystemUsageOverviewTechnicalSystemTypeReadModel",
                column: "TechnicalSystemTypeUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOverviewTechnicalSystemTypeReadModel_ParentId",
                table: "ItSystemUsageOverviewTechnicalSystemTypeReadModel",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the new read model table
            migrationBuilder.DropTable(
                name: "ItSystemUsageOverviewTechnicalSystemTypeReadModel");

            // Drop CSV column
            migrationBuilder.DropColumn(
                name: "TechnicalSystemTypeNamesAsCsv",
                table: "ItSystemUsageOverviewReadModels");

            // Restore old read model columns
            migrationBuilder.AddColumn<Guid>(
                name: "TechnicalSystemTypeUuid",
                table: "ItSystemUsageOverviewReadModels",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicalSystemTypeName",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_TechnicalSystemTypeUuid",
                table: "ItSystemUsageOverviewReadModels",
                column: "TechnicalSystemTypeUuid");

            // Restore the old FK column
            migrationBuilder.AddColumn<int>(
                name: "TechnicalSystemTypeId",
                table: "ItSystemUsage",
                type: "integer",
                nullable: true);

            // Migrate data back from join table (take first entry per usage)
            var migrateBackSql = @"
                    UPDATE dbo.""ItSystemUsage"" AS u
                    SET ""TechnicalSystemTypeId"" = jt.""TechnicalSystemTypesId""
                    FROM (
                        SELECT ""ReferencesId"", MIN(""TechnicalSystemTypesId"") AS ""TechnicalSystemTypesId""
                        FROM dbo.""ItSystemUsageTechnicalSystemTypes""
                        GROUP BY ""ReferencesId""
                    ) AS jt
                    WHERE u.""Id"" = jt.""ReferencesId""
                ";
            migrationBuilder.Sql(migrateBackSql);

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsage_TechnicalSystemTypeId",
                table: "ItSystemUsage",
                column: "TechnicalSystemTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItSystemUsage_TechnicalSystemTypes_TechnicalSystemTypeId",
                table: "ItSystemUsage",
                column: "TechnicalSystemTypeId",
                principalTable: "TechnicalSystemTypes",
                principalColumn: "Id");

            // Drop the join table
            migrationBuilder.DropTable(
                name: "ItSystemUsageTechnicalSystemTypes");
        }
    }
}
