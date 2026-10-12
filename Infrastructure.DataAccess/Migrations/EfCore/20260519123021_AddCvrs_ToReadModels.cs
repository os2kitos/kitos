using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class AddCvrs_ToReadModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ItSystemRightsHolderCvr",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierCvr",
                table: "ItContractOverviewReadModels",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ItSystemRightsHolderCvr",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "SupplierCvr",
                table: "ItContractOverviewReadModels");
        }
    }
}
