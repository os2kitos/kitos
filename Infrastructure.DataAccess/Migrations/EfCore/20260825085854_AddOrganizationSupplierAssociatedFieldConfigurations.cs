using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class AddOrganizationSupplierAssociatedFieldConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupplierAssociatedFieldConfiguration",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FieldKey = table.Column<string>(type: "text", nullable: false),
                    ControlState = table.Column<int>(type: "integer", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierAssociatedFieldConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierAssociatedFieldConfiguration_Organization_Organizat~",
                        column: x => x.OrganizationId,
                        principalSchema: "dbo",
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierAssociatedFieldConfiguration_User_LastChangedByUser~",
                        column: x => x.LastChangedByUserId,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierAssociatedFieldConfiguration_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierAssociatedFieldConfiguration_LastChangedByUserId",
                schema: "dbo",
                table: "SupplierAssociatedFieldConfiguration",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierAssociatedFieldConfiguration_ObjectOwnerId",
                schema: "dbo",
                table: "SupplierAssociatedFieldConfiguration",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_OrganizationId_SupplierAssociatedFieldConfiguration_FieldKey",
                schema: "dbo",
                table: "SupplierAssociatedFieldConfiguration",
                columns: new[] { "OrganizationId", "FieldKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupplierAssociatedFieldConfiguration",
                schema: "dbo");
        }
    }
}
