using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class AddContractSupplierReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasInternalSupplier",
                table: "ItContract",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ItContractSupplierOverviewReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false),
                    SupplierType = table.Column<int>(type: "integer", nullable: false),
                    SupplierUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SupplierCvr = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    IsSupplierDisabled = table.Column<bool>(type: "boolean", nullable: false),
                    HighestCriticalityUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    HighestCriticalityName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    HighestCriticalityRank = table.Column<int>(type: "integer", nullable: true),
                    ContractsAtHighestCriticalityCsv = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItContractSupplierOverviewReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItContractSupplierOverviewReadModels_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItContractSupplierOverviewAtCriticalityContractReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ContractId = table.Column<int>(type: "integer", nullable: false),
                    ContractUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItContractSupplierOverviewAtCriticalityContractReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItContractSupplierOverviewAtCriticalityContractReadModels_ItContractSupplierOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItContractSupplierOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Supplier_Read_Contracts_ContractId",
                table: "ItContractSupplierOverviewAtCriticalityContractReadModels",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Supplier_Read_Contracts_ContractName",
                table: "ItContractSupplierOverviewAtCriticalityContractReadModels",
                column: "ContractName");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Supplier_Read_Contracts_ContractUuid",
                table: "ItContractSupplierOverviewAtCriticalityContractReadModels",
                column: "ContractUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Supplier_Read_Contracts_ParentId",
                table: "ItContractSupplierOverviewAtCriticalityContractReadModels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Supplier_Read_HighestCriticalityName",
                table: "ItContractSupplierOverviewReadModels",
                column: "HighestCriticalityName");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Supplier_Read_HighestCriticalityRank",
                table: "ItContractSupplierOverviewReadModels",
                column: "HighestCriticalityRank");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Supplier_Read_HighestCriticalityUuid",
                table: "ItContractSupplierOverviewReadModels",
                column: "HighestCriticalityUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Supplier_Read_SupplierType",
                table: "ItContractSupplierOverviewReadModels",
                column: "SupplierType");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Supplier_Read_IsSupplierDisabled",
                table: "ItContractSupplierOverviewReadModels",
                column: "IsSupplierDisabled");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Supplier_Read_OrganizationId",
                table: "ItContractSupplierOverviewReadModels",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Supplier_Read_SupplierCvr",
                table: "ItContractSupplierOverviewReadModels",
                column: "SupplierCvr");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Supplier_Read_SupplierId",
                table: "ItContractSupplierOverviewReadModels",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Supplier_Read_SupplierName",
                table: "ItContractSupplierOverviewReadModels",
                column: "SupplierName");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Supplier_Read_SupplierUuid",
                table: "ItContractSupplierOverviewReadModels",
                column: "SupplierUuid");

            migrationBuilder.CreateIndex(
                name: "UX_ItContract_Supplier_Read_Org_Supplier",
                table: "ItContractSupplierOverviewReadModels",
                columns: new[] { "OrganizationId", "SupplierId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItContractSupplierOverviewAtCriticalityContractReadModels");

            migrationBuilder.DropTable(
                name: "ItContractSupplierOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "HasInternalSupplier",
                table: "ItContract");
        }
    }
}
