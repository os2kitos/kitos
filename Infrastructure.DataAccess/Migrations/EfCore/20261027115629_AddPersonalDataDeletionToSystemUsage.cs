using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class AddPersonalDataDeletionToSystemUsage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PersonalDataDeletionCapability",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalDataDeletionDeadlineTypeName",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PersonalDataDeletionDeadlineTypeUuid",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PersonalDataDeletionMethod",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalDataDeletionProcedureDocumentationName",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalDataDeletionProcedureDocumentationUrl",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PersonalDataDeletionResponsibleUserId",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalDataDeletionResponsibleUserName",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PersonalDataDeletionResponsibleUserUuid",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalDataDeletionVerificationDocumentationName",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalDataDeletionVerificationDocumentationUrl",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PersonalDataDeletionVerifiedDate",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PersonalDataDeletionCapability",
                schema: "dbo",
                table: "ItSystemUsage",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PersonalDataDeletionDeadlineTypeId",
                schema: "dbo",
                table: "ItSystemUsage",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PersonalDataDeletionMethod",
                schema: "dbo",
                table: "ItSystemUsage",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalDataDeletionProcedureDocumentationUrl",
                schema: "dbo",
                table: "ItSystemUsage",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalDataDeletionProcedureDocumentationUrlName",
                schema: "dbo",
                table: "ItSystemUsage",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PersonalDataDeletionResponsibleUserId",
                schema: "dbo",
                table: "ItSystemUsage",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalDataDeletionVerificationDocumentationUrl",
                schema: "dbo",
                table: "ItSystemUsage",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalDataDeletionVerificationDocumentationUrlName",
                schema: "dbo",
                table: "ItSystemUsage",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PersonalDataDeletionVerifiedDate",
                schema: "dbo",
                table: "ItSystemUsage",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LocalPersonalDataDeletionDeadlineTypes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    OptionId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalPersonalDataDeletionDeadlineTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalPersonalDataDeletionDeadlineTypes_Organization_Organiz~",
                        column: x => x.OrganizationId,
                        principalSchema: "dbo",
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalPersonalDataDeletionDeadlineTypes_User_LastChangedByUs~",
                        column: x => x.LastChangedByUserId,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalPersonalDataDeletionDeadlineTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PersonalDataDeletionDeadlineTypes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "citext", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalDataDeletionDeadlineTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalDataDeletionDeadlineTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PersonalDataDeletionDeadlineTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalSchema: "dbo",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsage_PersonalDataDeletionDeadlineTypeId",
                schema: "dbo",
                table: "ItSystemUsage",
                column: "PersonalDataDeletionDeadlineTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsage_PersonalDataDeletionResponsibleUserId",
                schema: "dbo",
                table: "ItSystemUsage",
                column: "PersonalDataDeletionResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalPersonalDataDeletionDeadlineTypes_LastChangedByUserId",
                schema: "dbo",
                table: "LocalPersonalDataDeletionDeadlineTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalPersonalDataDeletionDeadlineTypes_ObjectOwnerId",
                schema: "dbo",
                table: "LocalPersonalDataDeletionDeadlineTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalPersonalDataDeletionDeadlineTypes_OrganizationId",
                schema: "dbo",
                table: "LocalPersonalDataDeletionDeadlineTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalDataDeletionDeadlineTypes_LastChangedByUserId",
                schema: "dbo",
                table: "PersonalDataDeletionDeadlineTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonalDataDeletionDeadlineTypes_ObjectOwnerId",
                schema: "dbo",
                table: "PersonalDataDeletionDeadlineTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                schema: "dbo",
                table: "PersonalDataDeletionDeadlineTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ItSystemUsage_PersonalDataDeletionDeadlineTypes_PersonalDat~",
                schema: "dbo",
                table: "ItSystemUsage",
                column: "PersonalDataDeletionDeadlineTypeId",
                principalSchema: "dbo",
                principalTable: "PersonalDataDeletionDeadlineTypes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ItSystemUsage_User_PersonalDataDeletionResponsibleUserId",
                schema: "dbo",
                table: "ItSystemUsage",
                column: "PersonalDataDeletionResponsibleUserId",
                principalSchema: "dbo",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ItSystemUsage_PersonalDataDeletionDeadlineTypes_PersonalDat~",
                schema: "dbo",
                table: "ItSystemUsage");

            migrationBuilder.DropForeignKey(
                name: "FK_ItSystemUsage_User_PersonalDataDeletionResponsibleUserId",
                schema: "dbo",
                table: "ItSystemUsage");

            migrationBuilder.DropTable(
                name: "LocalPersonalDataDeletionDeadlineTypes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PersonalDataDeletionDeadlineTypes",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_ItSystemUsage_PersonalDataDeletionDeadlineTypeId",
                schema: "dbo",
                table: "ItSystemUsage");

            migrationBuilder.DropIndex(
                name: "IX_ItSystemUsage_PersonalDataDeletionResponsibleUserId",
                schema: "dbo",
                table: "ItSystemUsage");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionCapability",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionDeadlineTypeName",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionDeadlineTypeUuid",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionMethod",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionProcedureDocumentationName",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionProcedureDocumentationUrl",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionResponsibleUserId",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionResponsibleUserName",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionResponsibleUserUuid",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionVerificationDocumentationName",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionVerificationDocumentationUrl",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionVerifiedDate",
                schema: "dbo",
                table: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionCapability",
                schema: "dbo",
                table: "ItSystemUsage");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionDeadlineTypeId",
                schema: "dbo",
                table: "ItSystemUsage");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionMethod",
                schema: "dbo",
                table: "ItSystemUsage");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionProcedureDocumentationUrl",
                schema: "dbo",
                table: "ItSystemUsage");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionProcedureDocumentationUrlName",
                schema: "dbo",
                table: "ItSystemUsage");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionResponsibleUserId",
                schema: "dbo",
                table: "ItSystemUsage");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionVerificationDocumentationUrl",
                schema: "dbo",
                table: "ItSystemUsage");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionVerificationDocumentationUrlName",
                schema: "dbo",
                table: "ItSystemUsage");

            migrationBuilder.DropColumn(
                name: "PersonalDataDeletionVerifiedDate",
                schema: "dbo",
                table: "ItSystemUsage");
        }
    }
}
