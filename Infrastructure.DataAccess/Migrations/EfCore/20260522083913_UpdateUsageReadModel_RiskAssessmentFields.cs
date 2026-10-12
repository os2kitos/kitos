using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class UpdateUsageReadModel_RiskAssessmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RiskAssessmentConducted",
                table: "ItSystemUsageOverviewReadModels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RiskAssessmentResult",
                table: "ItSystemUsageOverviewReadModels",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RiskAssessmentConducted",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "RiskAssessmentResult",
                table: "ItSystemUsageOverviewReadModels");
        }
    }
}
