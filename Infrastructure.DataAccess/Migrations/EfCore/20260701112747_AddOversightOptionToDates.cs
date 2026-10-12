using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class AddOversightOptionToDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OversightOptionId",
                table: "DataProcessingRegistrationOversightDates",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrationOversightDates_OversightOptionId",
                table: "DataProcessingRegistrationOversightDates",
                column: "OversightOptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataProcessingRegistrationOversightDates_DataProcessingOversightOptions_OversightOptionId",
                table: "DataProcessingRegistrationOversightDates",
                column: "OversightOptionId",
                principalTable: "DataProcessingOversightOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataProcessingRegistrationOversightDates_DataProcessingOversightOptions_OversightOptionId",
                table: "DataProcessingRegistrationOversightDates");

            migrationBuilder.DropIndex(
                name: "IX_DataProcessingRegistrationOversightDates_OversightOptionId",
                table: "DataProcessingRegistrationOversightDates");

            migrationBuilder.DropColumn(
                name: "OversightOptionId",
                table: "DataProcessingRegistrationOversightDates");
        }
    }
}
