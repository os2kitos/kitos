using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class AddCriticalityLevelName_ToUsageReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CriticalityLevelDocumentationUrlName",
                table: "ItSystemUsageOverviewReadModels",
                newName: "SystemUsageCriticalityLevelName");

            migrationBuilder.AddColumn<string>(
                name: "CriticalityLevelDocumentationName",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CriticalityLevelDocumentationName",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.RenameColumn(
                name: "SystemUsageCriticalityLevelName",
                table: "ItSystemUsageOverviewReadModels",
                newName: "CriticalityLevelDocumentationUrlName");
        }
    }
}
