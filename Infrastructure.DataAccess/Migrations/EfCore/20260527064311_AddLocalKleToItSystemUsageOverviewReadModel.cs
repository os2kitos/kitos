using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class AddLocalKleToItSystemUsageOverviewReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {            migrationBuilder.AddColumn<string>(
                name: "LocalKleIdsAsCsv",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalKleNamesAsCsv",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ItSystemUsageOverviewLocalTaskRefReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    KLEId = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    KLEName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageOverviewLocalTaskRefReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOverviewLocalTaskRefReadModels_ItSystemUsageOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItSystemUsageOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewLocalTaskRefReadModel_Index_KLEId",
                table: "ItSystemUsageOverviewLocalTaskRefReadModels",
                column: "KLEId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewLocalTaskRefReadModel_Index_KLEName",
                table: "ItSystemUsageOverviewLocalTaskRefReadModels",
                column: "KLEName");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOverviewLocalTaskRefReadModels_ParentId",
                table: "ItSystemUsageOverviewLocalTaskRefReadModels",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItSystemUsageOverviewLocalTaskRefReadModels");

            migrationBuilder.DropColumn(
                name: "LocalKleIdsAsCsv",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "LocalKleNamesAsCsv",
                table: "ItSystemUsageOverviewReadModels");
        }
    }
}
