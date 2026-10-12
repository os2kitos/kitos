using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class MakeOrganizationUnitLocalIdIndexNotUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_LocalId",
                table: "OrganizationUnit");

            migrationBuilder.CreateIndex(
                name: "IX_LocalId",
                table: "OrganizationUnit",
                columns: new[] { "OrganizationId", "LocalId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LocalId",
                table: "OrganizationUnit");
            var filter = "\"LocalId\" IS NOT NULL";

            migrationBuilder.CreateIndex(
                name: "UX_LocalId",
                table: "OrganizationUnit",
                columns: new[] { "OrganizationId", "LocalId" },
                unique: true,
                filter: filter);
        }
    }
}
