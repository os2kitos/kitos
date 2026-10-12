using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class AddExternalAndInternalPaymentOrganizationUnits_ToContractReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalPaymentOrganizationUnitsCsv",
                table: "ItContractOverviewReadModels",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InternalPaymentOrganizationUnitsCsv",
                table: "ItContractOverviewReadModels",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalPaymentOrganizationUnitsCsv",
                table: "ItContractOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "InternalPaymentOrganizationUnitsCsv",
                table: "ItContractOverviewReadModels");
        }
    }
}
