using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class AddUsageReadModel_InterfacesIdsAndVersions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItInterfaceIdsAsCsv",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItInterfaceVersionsAsCsv",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItInterfaceIdsAsCsv",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "ItInterfaceVersionsAsCsv",
                table: "ItSystemUsageOverviewReadModels");
        }
    }
}
