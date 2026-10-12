using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class ExtendContractSupplierFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SupplierContactEmail",
                table: "ItContract",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierContactPerson",
                table: "ItContract",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierContactPhoneNumber",
                table: "ItContract",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierOrganizationUnitId",
                table: "ItContract",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "UseSupplierContractSignerAsContactPerson",
                table: "ItContract",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_SupplierOrganizationUnitId",
                table: "ItContract",
                column: "SupplierOrganizationUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_ItContract_OrganizationUnit_SupplierOrganizationUnitId",
                table: "ItContract",
                column: "SupplierOrganizationUnitId",
                principalTable: "OrganizationUnit",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItContract_OrganizationUnit_SupplierOrganizationUnitId",
                table: "ItContract");

            migrationBuilder.DropIndex(
                name: "IX_ItContract_SupplierOrganizationUnitId",
                table: "ItContract");

            migrationBuilder.DropColumn(
                name: "SupplierContactEmail",
                table: "ItContract");

            migrationBuilder.DropColumn(
                name: "SupplierContactPerson",
                table: "ItContract");

            migrationBuilder.DropColumn(
                name: "SupplierContactPhoneNumber",
                table: "ItContract");

            migrationBuilder.DropColumn(
                name: "SupplierOrganizationUnitId",
                table: "ItContract");

            migrationBuilder.DropColumn(
                name: "UseSupplierContractSignerAsContactPerson",
                table: "ItContract");
        }
    }
}
