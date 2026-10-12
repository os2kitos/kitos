using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class IntroduceArchivingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {            migrationBuilder.CreateTable(
                name: "ItSystemUsageArchive",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    SnapshotUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: false),
                    ArchivingDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ReferenceName = table.Column<string>(type: "text", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageArchive", x => x.Id);
                    table.UniqueConstraint("AK_ItSystemUsageArchive_Uuid", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageArchive_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageArchive_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageArchive_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveReference",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    ItSystemUsageArchiveUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveReference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveReference_ItSystemUsageArchive_ItSystemUsageArchiveUuid",
                        column: x => x.ItSystemUsageArchiveUuid,
                        principalTable: "ItSystemUsageArchive",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArchiveReference_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArchiveReference_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Snapshot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    TakenIntoUsageDate = table.Column<DateTime?>(type: "timestamp without time zone", nullable: true),
                    LegacyName = table.Column<string>(type: "text", nullable: true),
                    LocalName = table.Column<string>(type: "text", nullable: true),
                    LocalId = table.Column<string>(type: "text", nullable: true),
                    ItSystemUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ItSystemUsageArchiveUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Snapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Snapshot_ItSystemUsageArchive_ItSystemUsageArchiveUuid",
                        column: x => x.ItSystemUsageArchiveUuid,
                        principalTable: "ItSystemUsageArchive",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Snapshot_ItSystem_ItSystemUuid",
                        column: x => x.ItSystemUuid,
                        principalTable: "ItSystem",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Snapshot_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Snapshot_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveReference_ItSystemUsageArchiveUuid",
                table: "ArchiveReference",
                column: "ItSystemUsageArchiveUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveReference_LastChangedByUserId",
                table: "ArchiveReference",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveReference_ObjectOwnerId",
                table: "ArchiveReference",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_ArchiveReference_Uuid",
                table: "ArchiveReference",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageArchive_LastChangedByUserId",
                table: "ItSystemUsageArchive",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageArchive_ObjectOwnerId",
                table: "ItSystemUsageArchive",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageArchive_OrganizationId",
                table: "ItSystemUsageArchive",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "UX_ItSystemUsageArchive_ItSystemUsageSnapshotUuid",
                table: "ItSystemUsageArchive",
                column: "SnapshotUuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ItSystemUsageArchive_Uuid",
                table: "ItSystemUsageArchive",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageArchiveSnapshot_ItSystemUuid",
                table: "Snapshot",
                column: "ItSystemUuid");

            migrationBuilder.CreateIndex(
                name: "IX_Snapshot_LastChangedByUserId",
                table: "Snapshot",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Snapshot_ObjectOwnerId",
                table: "Snapshot",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_ItSystemUsageArchiveSnapshot_ItSystemUsageArchiveUuid",
                table: "Snapshot",
                column: "ItSystemUsageArchiveUuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_ItSystemUsageArchiveSnapshot_Uuid",
                table: "Snapshot",
                column: "Uuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArchiveReference");

            migrationBuilder.DropTable(
                name: "Snapshot");

            migrationBuilder.DropTable(
                name: "ItSystemUsageArchive");
        }
    }
}
