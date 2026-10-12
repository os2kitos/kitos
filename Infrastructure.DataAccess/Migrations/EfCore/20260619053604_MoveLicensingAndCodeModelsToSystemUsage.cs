using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class MoveLicensingAndCodeModelsToSystemUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicensingAndCodeModels",
                table: "ItSystem");

            migrationBuilder.AddColumn<string>(
                name: "LicensingAndCodeModels",
                table: "ItSystemUsageOverviewReadModels",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicensingAndCodeModels",
                table: "ItSystemUsage",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicensingAndCodeModels",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "LicensingAndCodeModels",
                table: "ItSystemUsage");

            migrationBuilder.AddColumn<string>(
                name: "LicensingAndCodeModels",
                table: "ItSystem",
                nullable: true);
        }
    }
}
