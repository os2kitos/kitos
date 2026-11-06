using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class AddContractInvoiceSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceDate1",
                schema: "dbo",
                table: "ItContractOverviewReadModels",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceDate2",
                schema: "dbo",
                table: "ItContractOverviewReadModels",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceDate3",
                schema: "dbo",
                table: "ItContractOverviewReadModels",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InvoiceDate4",
                schema: "dbo",
                table: "ItContractOverviewReadModels",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceFrequency",
                schema: "dbo",
                table: "ItContractOverviewReadModels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceFrequency",
                schema: "dbo",
                table: "ItContract",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ItContractInvoiceDates",
                schema: "dbo",
                columns: table => new
                {
                    ItContractId = table.Column<int>(type: "integer", nullable: false),
                    Period = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItContractInvoiceDates", x => new { x.ItContractId, x.Period });
                    table.ForeignKey(
                        name: "FK_ItContractInvoiceDates_ItContract_ItContractId",
                        column: x => x.ItContractId,
                        principalSchema: "dbo",
                        principalTable: "ItContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItContractInvoiceDates",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "InvoiceDate1",
                schema: "dbo",
                table: "ItContractOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "InvoiceDate2",
                schema: "dbo",
                table: "ItContractOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "InvoiceDate3",
                schema: "dbo",
                table: "ItContractOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "InvoiceDate4",
                schema: "dbo",
                table: "ItContractOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "InvoiceFrequency",
                schema: "dbo",
                table: "ItContractOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "InvoiceFrequency",
                schema: "dbo",
                table: "ItContract");
        }
    }
}
