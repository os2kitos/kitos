using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class AddArchivedByFieldToArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ArchivedById",
                table: "ItSystemUsageArchive",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageArchive_ArchivedById",
                table: "ItSystemUsageArchive",
                column: "ArchivedById");

            migrationBuilder.AddForeignKey(
                name: "FK_ItSystemUsageArchive_User_ArchivedById",
                table: "ItSystemUsageArchive",
                column: "ArchivedById",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItSystemUsageArchive_User_ArchivedById",
                table: "ItSystemUsageArchive");

            migrationBuilder.DropIndex(
                name: "IX_ItSystemUsageArchive_ArchivedById",
                table: "ItSystemUsageArchive");

            migrationBuilder.DropColumn(
                name: "ArchivedById",
                table: "ItSystemUsageArchive");
        }
    }
}
