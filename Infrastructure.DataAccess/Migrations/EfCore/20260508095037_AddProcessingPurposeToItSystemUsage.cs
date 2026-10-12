using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class AddProcessingPurposeToItSystemUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcessingPurpose",
                table: "ItSystemUsageOverviewReadModels",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessingPurpose",
                table: "ItSystemUsage",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ProcessingPurpose",
                table: "ItSystemUsageOverviewReadModels",
                column: "ProcessingPurpose");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ProcessingPurpose",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "ProcessingPurpose",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "ProcessingPurpose",
                table: "ItSystemUsage");
        }
    }
}
