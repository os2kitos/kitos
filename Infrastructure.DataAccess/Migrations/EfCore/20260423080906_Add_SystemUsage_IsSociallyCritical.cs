using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class Add_SystemUsage_IsSociallyCritical : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IsSociallyCritical",
                table: "ItSystemUsageOverviewReadModels",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IsSociallyCritical",
                table: "ItSystemUsage",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSociallyCritical",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "IsSociallyCritical",
                table: "ItSystemUsage");
        }
    }
}
