using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class Add_ItSystemUsage_CriticalityFieldsLastChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CriticalityFieldsLastChanged",
                table: "ItSystemUsageOverviewReadModels",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CriticalityFieldsLastChanged",
                table: "ItSystemUsage",
                type: "timestamp without time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CriticalityFieldsLastChanged",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "CriticalityFieldsLastChanged",
                table: "ItSystemUsage");
        }
    }
}
