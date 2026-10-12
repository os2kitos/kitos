using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class InitialBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BrokenExternalReferencesReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrokenExternalReferencesReports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PendingReadModelUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    SourceId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingReadModelUpdates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Salt = table.Column<string>(type: "text", nullable: false),
                    LastAdvisDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DeletedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultUserStartPreference = table.Column<string>(type: "text", nullable: true),
                    HasApiAccess = table.Column<bool>(type: "boolean", nullable: true),
                    HasStakeHolderAccess = table.Column<bool>(type: "boolean", nullable: false),
                    LockedOutDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false),
                    IsGlobalAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    IsSystemIntegrator = table.Column<bool>(type: "boolean", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_User_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Advice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationId = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Scheduling = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    AlarmDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    StopDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Body = table.Column<string>(type: "text", nullable: true),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    JobId = table.Column<string>(type: "text", nullable: true),
                    AdviceType = table.Column<int>(type: "integer", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Advice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Advice_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Advice_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AgreementElementTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgreementElementTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgreementElementTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgreementElementTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveLocation_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArchiveLocation_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArchiveTestLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveTestLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveTestLocation_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ArchiveTestLocation_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ArchiveTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchiveTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchiveTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArchiveTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AttachedOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectId = table.Column<int>(type: "integer", nullable: false),
                    ObjectType = table.Column<int>(type: "integer", nullable: false),
                    OptionId = table.Column<int>(type: "integer", nullable: false),
                    OptionType = table.Column<int>(type: "integer", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachedOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttachedOptions_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttachedOptions_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BusinessTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BusinessTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContactPersons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Name = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactPersons_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContactPersons_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CountryCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CountryCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CountryCodes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CountryCodes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CriticalityTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriticalityTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriticalityTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CriticalityTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataProcessingBasisForTransferOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingBasisForTransferOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataProcessingBasisForTransferOptions_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataProcessingBasisForTransferOptions_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataProcessingCountryOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingCountryOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataProcessingCountryOptions_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataProcessingCountryOptions_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataProcessingDataResponsibleOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingDataResponsibleOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataProcessingDataResponsibleOptions_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataProcessingDataResponsibleOptions_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataProcessingOversightOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingOversightOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataProcessingOversightOptions_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataProcessingOversightOptions_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataProcessingRegistrationRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    HasReadAccess = table.Column<bool>(type: "boolean", nullable: false),
                    HasWriteAccess = table.Column<bool>(type: "boolean", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingRegistrationRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataProcessingRegistrationRoles_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataProcessingRegistrationRoles_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HelpTexts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Title = table.Column<string>(type: "text", nullable: true),
                    Key = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpTexts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HelpTexts_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HelpTexts_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InterfaceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterfaceTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InterfaceTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InterfaceTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItContractRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    HasReadAccess = table.Column<bool>(type: "boolean", nullable: false),
                    HasWriteAccess = table.Column<bool>(type: "boolean", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItContractRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItContractRoles_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItContractRoles_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItContractTemplateTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItContractTemplateTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItContractTemplateTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItContractTemplateTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItContractTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItContractTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItContractTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItContractTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemCategories_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystemCategories_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    HasReadAccess = table.Column<bool>(type: "boolean", nullable: false),
                    HasWriteAccess = table.Column<bool>(type: "boolean", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemRoles_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystemRoles_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KLEUpdateHistoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Version = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KLEUpdateHistoryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KLEUpdateHistoryItems_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KLEUpdateHistoryItems_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OptionExtendTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionExtendTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionExtendTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OptionExtendTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUnitRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    HasReadAccess = table.Column<bool>(type: "boolean", nullable: false),
                    HasWriteAccess = table.Column<bool>(type: "boolean", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUnitRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUnitRoles_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationUnitRoles_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PasswordResetRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Hash = table.Column<string>(type: "text", nullable: true),
                    Time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasswordResetRequest_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PasswordResetRequest_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PasswordResetRequest_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentFreqencyTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentFreqencyTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentFreqencyTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentFreqencyTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentModelTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentModelTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentModelTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentModelTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PriceRegulationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceRegulationTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceRegulationTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PriceRegulationTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcurementStrategyTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcurementStrategyTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcurementStrategyTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcurementStrategyTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PublicMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LongDescription = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: true),
                    ShortDescription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Link = table.Column<string>(type: "text", nullable: true),
                    IconType = table.Column<int>(type: "integer", nullable: true),
                    IsMain = table.Column<bool>(type: "boolean", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicMessages_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PublicMessages_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseFormTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseFormTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseFormTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseFormTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegisterTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisterTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegisterTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RegisterTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RelationFrequencyTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationFrequencyTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelationFrequencyTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RelationFrequencyTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SensitiveDataTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensitiveDataTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensitiveDataTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SensitiveDataTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SensitivePersonalDataTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensitivePersonalDataTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensitivePersonalDataTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SensitivePersonalDataTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SsoUserIdentities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ExternalUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    User_Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SsoUserIdentities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SsoUserIdentities_User_User_Id",
                        column: x => x.User_Id,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TerminationDeadlineTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsLocallyAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatory = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminationDeadlineTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TerminationDeadlineTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TerminationDeadlineTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Text",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Value = table.Column<string>(type: "text", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Text", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Text_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Text_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdviceSents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    AdviceSentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AdviceId = table.Column<int>(type: "integer", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdviceSent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdviceSent_Advice_AdviceId",
                        column: x => x.AdviceId,
                        principalTable: "Advice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdviceSent_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdviceSent_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Organization",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Adress = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    Cvr = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    ForeignCvr = table.Column<string>(type: "text", nullable: true),
                    ForeignCountryCodeId = table.Column<int>(type: "integer", nullable: true),
                    AccessModifier = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false),
                    ContactPersonId = table.Column<int>(type: "integer", nullable: true),
                    ContactPerson_Id = table.Column<int>(type: "integer", nullable: true),
                    IsDefaultOrganization = table.Column<bool>(type: "boolean", nullable: true),
                    IsSupplier = table.Column<bool>(type: "boolean", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organization_ContactPersons_ContactPerson_Id",
                        column: x => x.ContactPerson_Id,
                        principalTable: "ContactPersons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Organization_CountryCodes_ForeignCountryCodeId",
                        column: x => x.ForeignCountryCodeId,
                        principalTable: "CountryCodes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Organization_OrganizationTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "OrganizationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Organization_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdviceUserRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    AdviceId = table.Column<int>(type: "integer", nullable: true),
                    ItContractRoleId = table.Column<int>(type: "integer", nullable: true),
                    ItSystemRoleId = table.Column<int>(type: "integer", nullable: true),
                    DataProcessingRegistrationRoleId = table.Column<int>(type: "integer", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    RecieverType = table.Column<int>(type: "integer", nullable: false),
                    RecpientType = table.Column<int>(type: "integer", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdviceUserRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdviceUserRelations_Advice_AdviceId",
                        column: x => x.AdviceId,
                        principalTable: "Advice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdviceUserRelations_DataProcessingRegistrationRoles_DataProcessingRegistrationRoleId",
                        column: x => x.DataProcessingRegistrationRoleId,
                        principalTable: "DataProcessingRegistrationRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdviceUserRelations_ItContractRoles_ItContractRoleId",
                        column: x => x.ItContractRoleId,
                        principalTable: "ItContractRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdviceUserRelations_ItSystemRoles_ItSystemRoleId",
                        column: x => x.ItSystemRoleId,
                        principalTable: "ItSystemRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdviceUserRelations_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdviceUserRelations_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Config",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ShowItSystemModule = table.Column<bool>(type: "boolean", nullable: false),
                    ShowItContractModule = table.Column<bool>(type: "boolean", nullable: false),
                    ShowDataProcessing = table.Column<bool>(type: "boolean", nullable: false),
                    ShowItSystemPrefix = table.Column<bool>(type: "boolean", nullable: false),
                    ShowItContractPrefix = table.Column<bool>(type: "boolean", nullable: false),
                    ItSupportModuleNameId = table.Column<int>(type: "integer", nullable: false),
                    ItSupportGuide = table.Column<string>(type: "text", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Config", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Config_Organization_Id",
                        column: x => x.Id,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Config_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Config_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataProtectionAdvisors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Name = table.Column<string>(type: "text", nullable: true),
                    Cvr = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Adress = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionAdvisors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataProtectionAdvisors_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataProtectionAdvisors_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataProtectionAdvisors_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataResponsibles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Name = table.Column<string>(type: "text", nullable: true),
                    Cvr = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Adress = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataResponsibles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataResponsibles_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DataResponsibles_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataResponsibles_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItInterface",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Url = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ItInterfaceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    InterfaceId = table.Column<int>(type: "integer", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AccessModifier = table.Column<int>(type: "integer", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItInterface", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItInterface_InterfaceTypes_InterfaceId",
                        column: x => x.InterfaceId,
                        principalTable: "InterfaceTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItInterface_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItInterface_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItInterface_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KendoOrganizationalConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    OverviewType = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KendoOrganizationalConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KendoOrganizationalConfigurations_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KendoOrganizationalConfigurations_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_KendoOrganizationalConfigurations_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LifeCycleTrackingEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EntityUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<int>(type: "integer", nullable: false),
                    OptionalOrganizationReferenceId = table.Column<int>(type: "integer", nullable: true),
                    OptionalAccessModifier = table.Column<int>(type: "integer", nullable: true),
                    OptionalRightsHolderOrganizationId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LifeCycleTrackingEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LifeCycleTrackingEvents_Organization_OptionalOrganizationReferenceId",
                        column: x => x.OptionalOrganizationReferenceId,
                        principalTable: "Organization",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LifeCycleTrackingEvents_Organization_OptionalRightsHolderOrganizationId",
                        column: x => x.OptionalRightsHolderOrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LifeCycleTrackingEvents_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalAgreementElementTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalAgreementElementTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalAgreementElementTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalAgreementElementTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalAgreementElementTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalArchiveLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalArchiveLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalArchiveLocations_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalArchiveLocations_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalArchiveLocations_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalArchiveTestLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalArchiveTestLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalArchiveTestLocations_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalArchiveTestLocations_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalArchiveTestLocations_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalArchiveTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalArchiveTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalArchiveTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalArchiveTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalArchiveTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalBusinessTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalBusinessTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalBusinessTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalBusinessTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalBusinessTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalCriticalityTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalCriticalityTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalCriticalityTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalCriticalityTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalCriticalityTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalDataProcessingBasisForTransferOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalDataProcessingBasisForTransferOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalDataProcessingBasisForTransferOptions_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalDataProcessingBasisForTransferOptions_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalDataProcessingBasisForTransferOptions_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalDataProcessingCountryOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalDataProcessingCountryOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalDataProcessingCountryOptions_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalDataProcessingCountryOptions_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalDataProcessingCountryOptions_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalDataProcessingDataResponsibleOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalDataProcessingDataResponsibleOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalDataProcessingDataResponsibleOptions_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalDataProcessingDataResponsibleOptions_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalDataProcessingDataResponsibleOptions_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalDataProcessingOversightOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalDataProcessingOversightOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalDataProcessingOversightOptions_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalDataProcessingOversightOptions_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalDataProcessingOversightOptions_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalDataProcessingRegistrationRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    OptionId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsExternallyUsed = table.Column<bool>(type: "boolean", nullable: false),
                    ExternallyUsedDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalDataProcessingRegistrationRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalDataProcessingRegistrationRoles_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalDataProcessingRegistrationRoles_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalDataProcessingRegistrationRoles_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalDataTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalDataTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalDataTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalDataTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalDataTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalRelationFrequencyTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalFrequencyTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalFrequencyTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalFrequencyTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalFrequencyTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalInterfaceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalInterfaceTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalInterfaceTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalInterfaceTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalInterfaceTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalItContractRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    OptionId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsExternallyUsed = table.Column<bool>(type: "boolean", nullable: false),
                    ExternallyUsedDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalItContractRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalItContractRoles_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalItContractRoles_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalItContractRoles_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalItContractTemplateTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalItContractTemplateTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalItContractTemplateTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalItContractTemplateTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalItContractTemplateTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalItContractTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalItContractTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalItContractTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalItContractTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalItContractTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalItSystemCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalItSystemCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalItSystemCategories_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalItSystemCategories_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalItSystemCategories_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalItSystemRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    OptionId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsExternallyUsed = table.Column<bool>(type: "boolean", nullable: false),
                    ExternallyUsedDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalItSystemRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalItSystemRoles_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalItSystemRoles_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalItSystemRoles_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalOptionExtendTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalOptionExtendTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalOptionExtendTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalOptionExtendTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalOptionExtendTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalOrganizationUnitRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    OptionId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsExternallyUsed = table.Column<bool>(type: "boolean", nullable: false),
                    ExternallyUsedDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalOrganizationUnitRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalOrganizationUnitRoles_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalOrganizationUnitRoles_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalOrganizationUnitRoles_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalPaymentFreqencyTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalPaymentFreqencyTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalPaymentFreqencyTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalPaymentFreqencyTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalPaymentFreqencyTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalPaymentModelTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalPaymentModelTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalPaymentModelTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalPaymentModelTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalPaymentModelTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalPriceRegulationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalPriceRegulationTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalPriceRegulationTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalPriceRegulationTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalPriceRegulationTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalProcurementStrategyTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalProcurementStrategyTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalProcurementStrategyTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalProcurementStrategyTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalProcurementStrategyTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalPurchaseFormTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalPurchaseFormTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalPurchaseFormTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalPurchaseFormTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalPurchaseFormTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalRegisterTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalRegisterTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalRegisterTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalRegisterTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalRegisterTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalSensitiveDataTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalSensitiveDataTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalSensitiveDataTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalSensitiveDataTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalSensitiveDataTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalSensitivePersonalDataTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalSensitivePersonalDataTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalSensitivePersonalDataTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalSensitivePersonalDataTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalSensitivePersonalDataTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocalTerminationDeadlineTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
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
                    table.PrimaryKey("PK_LocalTerminationDeadlineTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalTerminationDeadlineTypes_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocalTerminationDeadlineTypes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LocalTerminationDeadlineTypes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrganizationSuppliers",
                columns: table => new
                {
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationSuppliers", x => new { x.SupplierId, x.OrganizationId });
                    table.ForeignKey(
                        name: "FK_OrganizationSuppliers_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationSuppliers_Organization_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUnit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Origin = table.Column<int>(type: "integer", nullable: false),
                    ExternalOriginUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    LocalId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Ean = table.Column<long>(type: "bigint", nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUnit_OrganizationUnit_ParentId",
                        column: x => x.ParentId,
                        principalTable: "OrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationUnit_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationUnit_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationUnit_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StsOrganizationIdentities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ExternalUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SsoOrganizationIdentities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SsoOrganizationIdentities_Organization_Organization_Id",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StsOrganizationConnections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    Connected = table.Column<bool>(type: "boolean", nullable: false),
                    SynchronizationDepth = table.Column<int>(type: "integer", nullable: true),
                    SubscribeToUpdates = table.Column<bool>(type: "boolean", nullable: false),
                    DateOfLatestCheckBySubscription = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StsOrganizationConnections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StsOrganizationConnections_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StsOrganizationConnections_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StsOrganizationConnections_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UIModuleCustomizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    Module = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UIModuleCustomizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UIModuleCustomizations_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UIModuleCustomizations_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UIModuleCustomizations_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BrokenLinkInInterfaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ValueOfCheckedUrl = table.Column<string>(type: "text", nullable: true),
                    Cause = table.Column<int>(type: "integer", nullable: false),
                    ErrorResponseCode = table.Column<int>(type: "integer", nullable: true),
                    ReferenceDateOfLatestLinkChange = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    BrokenReferenceOrigin_Id = table.Column<int>(type: "integer", nullable: false),
                    ParentReport_Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrokenLinkInInterface", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrokenLinkInInterface_BrokenExternalReferencesReports_ParentReport_Id",
                        column: x => x.ParentReport_Id,
                        principalTable: "BrokenExternalReferencesReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BrokenLinkInInterface_ItInterface_BrokenReferenceOrigin_Id",
                        column: x => x.BrokenReferenceOrigin_Id,
                        principalTable: "ItInterface",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataRow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ItInterfaceId = table.Column<int>(type: "integer", nullable: false),
                    DataTypeId = table.Column<int>(type: "integer", nullable: true),
                    Data = table.Column<string>(type: "text", nullable: true),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataRow", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataRow_DataTypes_DataTypeId",
                        column: x => x.DataTypeId,
                        principalTable: "DataTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DataRow_ItInterface_ItInterfaceId",
                        column: x => x.ItInterfaceId,
                        principalTable: "ItInterface",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataRow_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataRow_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "KendoColumnConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    PersistId = table.Column<string>(type: "text", nullable: true),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    KendoOrganizationalConfigurationId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KendoColumnConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KendoColumnConfiguration_KendoOrganizationalConfigurations_KendoOrganizationalConfigurationId",
                        column: x => x.KendoOrganizationalConfigurationId,
                        principalTable: "KendoOrganizationalConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationRights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    DefaultOrgUnitId = table.Column<int>(type: "integer", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationRights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationRights_OrganizationUnit_DefaultOrgUnitId",
                        column: x => x.DefaultOrgUnitId,
                        principalTable: "OrganizationUnit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OrganizationRights_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationRights_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationRights_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationRights_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUnitRights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ObjectId = table.Column<int>(type: "integer", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUnitRights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUnitRights_OrganizationUnitRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "OrganizationUnitRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationUnitRights_OrganizationUnit_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "OrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrganizationUnitRights_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationUnitRights_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationUnitRights_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskRef",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: true),
                    TaskKey = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    Description = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ActiveFrom = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ActiveTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    OwnedByOrganizationUnitId = table.Column<int>(type: "integer", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskRef", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskRef_OrganizationUnit_OwnedByOrganizationUnitId",
                        column: x => x.OwnedByOrganizationUnitId,
                        principalTable: "OrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskRef_TaskRef_ParentId",
                        column: x => x.ParentId,
                        principalTable: "TaskRef",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskRef_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TaskRef_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StsOrganizationChangeLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    StsOrganizationConnectionId = table.Column<int>(type: "integer", nullable: false),
                    ResponsibleUserId = table.Column<int>(type: "integer", nullable: true),
                    ResponsibleType = table.Column<int>(type: "integer", nullable: false),
                    LogTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StsOrganizationChangeLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StsOrganizationChangeLogs_StsOrganizationConnections_StsOrganizationConnectionId",
                        column: x => x.StsOrganizationConnectionId,
                        principalTable: "StsOrganizationConnections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StsOrganizationChangeLogs_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StsOrganizationChangeLogs_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StsOrganizationChangeLogs_User_ResponsibleUserId",
                        column: x => x.ResponsibleUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomizedUiNodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ModuleId = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomizedUiNodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomizedUiNodes_UIModuleCustomizations_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "UIModuleCustomizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomizedUiNodes_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CustomizedUiNodes_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StsOrganizationConsequenceLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ChangeLogId = table.Column<int>(type: "integer", nullable: false),
                    ExternalUnitUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StsOrganizationConsequenceLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StsOrganizationConsequenceLogs_StsOrganizationChangeLogs_ChangeLogId",
                        column: x => x.ChangeLogId,
                        principalTable: "StsOrganizationChangeLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StsOrganizationConsequenceLogs_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StsOrganizationConsequenceLogs_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArchivePeriod",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UniqueArchiveId = table.Column<string>(type: "text", nullable: true),
                    ItSystemUsageId = table.Column<int>(type: "integer", nullable: false),
                    Approved = table.Column<bool>(type: "boolean", nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArchivePeriod", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArchivePeriod_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ArchivePeriod_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BrokenLinkInExternalReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ValueOfCheckedUrl = table.Column<string>(type: "text", nullable: true),
                    Cause = table.Column<int>(type: "integer", nullable: false),
                    ErrorResponseCode = table.Column<int>(type: "integer", nullable: true),
                    ReferenceDateOfLatestLinkChange = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    BrokenReferenceOrigin_Id = table.Column<int>(type: "integer", nullable: false),
                    ParentReport_Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrokenLinkInExternalReference", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BrokenLinkInExternalReference_BrokenExternalReferencesReports_ParentReport_Id",
                        column: x => x.ParentReport_Id,
                        principalTable: "BrokenExternalReferencesReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataProcessingRegistrationDataProcessingCountryOptions",
                columns: table => new
                {
                    DataProcessingRegistration_Id = table.Column<int>(type: "integer", nullable: false),
                    DataProcessingCountryOption_Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingCountryOptionDataProcessingRegistration", x => new { x.DataProcessingRegistration_Id, x.DataProcessingCountryOption_Id });
                    table.ForeignKey(
                        name: "FK_DataProcessingCountryOptionDataProcessingRegistration_DataProcessingCountryOptions_DataProcessingCountryOption_Id",
                        column: x => x.DataProcessingCountryOption_Id,
                        principalTable: "DataProcessingCountryOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataProcessingRegistrationDataProcessingOversightOptions",
                columns: table => new
                {
                    DataProcessingRegistration_Id = table.Column<int>(type: "integer", nullable: false),
                    DataProcessingOversightOption_Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingOversightOptionDataProcessingRegistration", x => new { x.DataProcessingRegistration_Id, x.DataProcessingOversightOption_Id });
                    table.ForeignKey(
                        name: "FK_DataProcessingOversightOptionDataProcessingRegistration_DataProcessingOversightOptions_DataProcessingOversightOption_Id",
                        column: x => x.DataProcessingOversightOption_Id,
                        principalTable: "DataProcessingOversightOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItContractDataProcessingRegistrations",
                columns: table => new
                {
                    ItContract_Id = table.Column<int>(type: "integer", nullable: false),
                    DataProcessingRegistration_Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingRegistrationItContract", x => new { x.ItContract_Id, x.DataProcessingRegistration_Id });
                });

            migrationBuilder.CreateTable(
                name: "DataProcessingRegistrationItSystemUsages",
                columns: table => new
                {
                    DataProcessingRegistration_Id = table.Column<int>(type: "integer", nullable: false),
                    ItSystemUsage_Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingRegistrationItSystemUsage", x => new { x.DataProcessingRegistration_Id, x.ItSystemUsage_Id });
                });

            migrationBuilder.CreateTable(
                name: "DataProcessingRegistrationOrganizations",
                columns: table => new
                {
                    DataProcessingRegistration_Id = table.Column<int>(type: "integer", nullable: false),
                    Organization_Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingRegistrationOrganization", x => new { x.DataProcessingRegistration_Id, x.Organization_Id });
                    table.ForeignKey(
                        name: "FK_DataProcessingRegistrationOrganization_Organization_Organization_Id",
                        column: x => x.Organization_Id,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataProcessingRegistrationOversightDates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    OversightDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    OversightRemark = table.Column<string>(type: "text", nullable: true),
                    OversightReportLink = table.Column<string>(type: "text", nullable: true),
                    OversightReportLinkName = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingRegistrationOversightDates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataProcessingRegistrationReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    SourceEntityId = table.Column<int>(type: "integer", nullable: false),
                    SourceEntityUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    MainReferenceUserAssignedId = table.Column<string>(type: "text", nullable: true),
                    MainReferenceUrl = table.Column<string>(type: "text", nullable: true),
                    MainReferenceTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SystemNamesAsCsv = table.Column<string>(type: "text", nullable: true),
                    SystemUuidsAsCsv = table.Column<string>(type: "text", nullable: true),
                    DataProcessorNamesAsCsv = table.Column<string>(type: "text", nullable: true),
                    SubDataProcessorNamesAsCsv = table.Column<string>(type: "text", nullable: true),
                    IsAgreementConcluded = table.Column<int>(type: "integer", nullable: true),
                    TransferToInsecureThirdCountries = table.Column<int>(type: "integer", nullable: true),
                    AgreementConcludedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LatestOversightDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LatestOversightRemark = table.Column<string>(type: "text", nullable: true),
                    LatestOversightReportLink = table.Column<string>(type: "text", nullable: true),
                    LatestOversightReportLinkName = table.Column<string>(type: "text", nullable: true),
                    BasisForTransfer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BasisForTransferUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    OversightInterval = table.Column<int>(type: "integer", nullable: true),
                    DataResponsible = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataResponsibleUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    OversightOptionNamesAsCsv = table.Column<string>(type: "text", nullable: true),
                    IsOversightCompleted = table.Column<int>(type: "integer", nullable: true),
                    OversightScheduledInspectionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ActiveAccordingToMainContract = table.Column<bool>(type: "boolean", nullable: false),
                    ContractNamesAsCsv = table.Column<string>(type: "text", nullable: true),
                    LastChangedById = table.Column<int>(type: "integer", nullable: true),
                    LastChangedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastChangedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ResponsibleOrgUnitUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponsibleOrgUnitId = table.Column<int>(type: "integer", nullable: true),
                    ResponsibleOrgUnitName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingRegistrationReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataProcessingRegistrationReadModels_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataProcessingRegistrationRoleAssignmentReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Email = table.Column<string>(type: "text", nullable: true),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    UserFullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingRegistrationRoleAssignmentReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataProcessingRegistrationRoleAssignmentReadModels_DataProcessingRegistrationReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "DataProcessingRegistrationReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DataProcessingRegistrationRights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ObjectId = table.Column<int>(type: "integer", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingRegistrationRights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataProcessingRegistrationRights_DataProcessingRegistrationRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "DataProcessingRegistrationRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataProcessingRegistrationRights_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataProcessingRegistrationRights_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataProcessingRegistrationRights_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DataProcessingRegistrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    HasSubDataProcessors = table.Column<int>(type: "integer", nullable: true),
                    TransferToInsecureThirdCountries = table.Column<int>(type: "integer", nullable: true),
                    DataResponsible_Id = table.Column<int>(type: "integer", nullable: true),
                    DataResponsibleRemark = table.Column<string>(type: "text", nullable: true),
                    OversightOptionRemark = table.Column<string>(type: "text", nullable: true),
                    ReferenceId = table.Column<int>(type: "integer", nullable: true),
                    IsAgreementConcluded = table.Column<int>(type: "integer", nullable: true),
                    AgreementConcludedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AgreementConcludedRemark = table.Column<string>(type: "text", nullable: true),
                    BasisForTransferId = table.Column<int>(type: "integer", nullable: true),
                    OversightInterval = table.Column<int>(type: "integer", nullable: true),
                    OversightIntervalRemark = table.Column<string>(type: "text", nullable: true),
                    ResponsibleOrganizationUnitId = table.Column<int>(type: "integer", nullable: true),
                    IsOversightCompleted = table.Column<int>(type: "integer", nullable: true),
                    OversightCompletedRemark = table.Column<string>(type: "text", nullable: true),
                    OversightScheduledInspectionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MainContractId = table.Column<int>(type: "integer", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProcessingRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataProcessingRegistrations_DataProcessingBasisForTransferOptions_BasisForTransferId",
                        column: x => x.BasisForTransferId,
                        principalTable: "DataProcessingBasisForTransferOptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DataProcessingRegistrations_DataProcessingDataResponsibleOptions_DataResponsible_Id",
                        column: x => x.DataResponsible_Id,
                        principalTable: "DataProcessingDataResponsibleOptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DataProcessingRegistrations_OrganizationUnit_ResponsibleOrganizationUnitId",
                        column: x => x.ResponsibleOrganizationUnitId,
                        principalTable: "OrganizationUnit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DataProcessingRegistrations_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DataProcessingRegistrations_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DataProcessingRegistrations_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SubDataProcessors",
                columns: table => new
                {
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    DataProcessingRegistrationId = table.Column<int>(type: "integer", nullable: false),
                    SubDataProcessorBasisForTransferId = table.Column<int>(type: "integer", nullable: true),
                    TransferToInsecureCountry = table.Column<int>(type: "integer", nullable: true),
                    InsecureCountryId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubDataProcessors", x => new { x.OrganizationId, x.DataProcessingRegistrationId });
                    table.ForeignKey(
                        name: "FK_SubDataProcessors_DataProcessingBasisForTransferOptions_SubDataProcessorBasisForTransferId",
                        column: x => x.SubDataProcessorBasisForTransferId,
                        principalTable: "DataProcessingBasisForTransferOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubDataProcessors_DataProcessingCountryOptions_InsecureCountryId",
                        column: x => x.InsecureCountryId,
                        principalTable: "DataProcessingCountryOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubDataProcessors_DataProcessingRegistrations_DataProcessingRegistrationId",
                        column: x => x.DataProcessingRegistrationId,
                        principalTable: "DataProcessingRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubDataProcessors_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EconomyStream",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ExternPaymentForId = table.Column<int>(type: "integer", nullable: true),
                    InternPaymentForId = table.Column<int>(type: "integer", nullable: true),
                    OrganizationUnitId = table.Column<int>(type: "integer", nullable: true),
                    Acquisition = table.Column<int>(type: "integer", nullable: false),
                    Operation = table.Column<int>(type: "integer", nullable: false),
                    Other = table.Column<int>(type: "integer", nullable: false),
                    AccountingEntry = table.Column<string>(type: "text", nullable: true),
                    AuditStatus = table.Column<int>(type: "integer", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EconomyStream", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EconomyStream_OrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalTable: "OrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EconomyStream_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EconomyStream_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Exhibit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    ItSystemId = table.Column<int>(type: "integer", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exhibit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exhibit_ItInterface_Id",
                        column: x => x.Id,
                        principalTable: "ItInterface",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exhibit_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Exhibit_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExternalReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ItContract_Id = table.Column<int>(type: "integer", nullable: true),
                    ItSystemUsage_Id = table.Column<int>(type: "integer", nullable: true),
                    ItSystem_Id = table.Column<int>(type: "integer", nullable: true),
                    DataProcessingRegistration_Id = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "text", nullable: true),
                    ExternalReferenceId = table.Column<string>(type: "text", nullable: true),
                    URL = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalReferences_DataProcessingRegistrations_DataProcessingRegistration_Id",
                        column: x => x.DataProcessingRegistration_Id,
                        principalTable: "DataProcessingRegistrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExternalReferences_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExternalReferences_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItContract",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferenceId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ItContractId = table.Column<string>(type: "text", nullable: true),
                    SupplierContractSigner = table.Column<string>(type: "text", nullable: true),
                    HasSupplierSigned = table.Column<bool>(type: "boolean", nullable: false),
                    SupplierSignedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ContractSigner = table.Column<string>(type: "text", nullable: true),
                    IsSigned = table.Column<bool>(type: "boolean", nullable: false),
                    SignedDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ResponsibleOrganizationUnitId = table.Column<int>(type: "integer", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    ProcurementStrategyId = table.Column<int>(type: "integer", nullable: true),
                    ProcurementPlanQuarter = table.Column<int>(type: "integer", nullable: true),
                    ProcurementPlanYear = table.Column<int>(type: "integer", nullable: true),
                    ProcurementInitiated = table.Column<int>(type: "integer", nullable: true),
                    ContractTemplateId = table.Column<int>(type: "integer", nullable: true),
                    ContractTypeId = table.Column<int>(type: "integer", nullable: true),
                    PurchaseFormId = table.Column<int>(type: "integer", nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    RequireValidParent = table.Column<bool>(type: "boolean", nullable: false),
                    CriticalityId = table.Column<int>(type: "integer", nullable: true),
                    Concluded = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DurationYears = table.Column<int>(type: "integer", nullable: true),
                    DurationMonths = table.Column<int>(type: "integer", nullable: true),
                    DurationOngoing = table.Column<bool>(type: "boolean", nullable: false),
                    IrrevocableTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Terminated = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TerminationDeadlineId = table.Column<int>(type: "integer", nullable: true),
                    OptionExtendId = table.Column<int>(type: "integer", nullable: true),
                    ExtendMultiplier = table.Column<int>(type: "integer", nullable: false),
                    Running = table.Column<int>(type: "integer", nullable: true),
                    ByEnding = table.Column<int>(type: "integer", nullable: true),
                    OperationRemunerationBegun = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PaymentFreqencyId = table.Column<int>(type: "integer", nullable: true),
                    PaymentModelId = table.Column<int>(type: "integer", nullable: true),
                    PriceRegulationId = table.Column<int>(type: "integer", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItContract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItContract_CriticalityTypes_CriticalityId",
                        column: x => x.CriticalityId,
                        principalTable: "CriticalityTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItContract_ExternalReferences_ReferenceId",
                        column: x => x.ReferenceId,
                        principalTable: "ExternalReferences",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItContract_ItContractTemplateTypes_ContractTemplateId",
                        column: x => x.ContractTemplateId,
                        principalTable: "ItContractTemplateTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItContract_ItContractTypes_ContractTypeId",
                        column: x => x.ContractTypeId,
                        principalTable: "ItContractTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItContract_ItContract_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItContract_OptionExtendTypes_OptionExtendId",
                        column: x => x.OptionExtendId,
                        principalTable: "OptionExtendTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItContract_OrganizationUnit_ResponsibleOrganizationUnitId",
                        column: x => x.ResponsibleOrganizationUnitId,
                        principalTable: "OrganizationUnit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItContract_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItContract_Organization_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Organization",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItContract_PaymentFreqencyTypes_PaymentFreqencyId",
                        column: x => x.PaymentFreqencyId,
                        principalTable: "PaymentFreqencyTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItContract_PaymentModelTypes_PaymentModelId",
                        column: x => x.PaymentModelId,
                        principalTable: "PaymentModelTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItContract_PriceRegulationTypes_PriceRegulationId",
                        column: x => x.PriceRegulationId,
                        principalTable: "PriceRegulationTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItContract_ProcurementStrategyTypes_ProcurementStrategyId",
                        column: x => x.ProcurementStrategyId,
                        principalTable: "ProcurementStrategyTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItContract_PurchaseFormTypes_PurchaseFormId",
                        column: x => x.PurchaseFormId,
                        principalTable: "PurchaseFormTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItContract_TerminationDeadlineTypes_TerminationDeadlineId",
                        column: x => x.TerminationDeadlineId,
                        principalTable: "TerminationDeadlineTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItContract_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItContract_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ItSystem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    BelongsToId = table.Column<int>(type: "integer", nullable: true),
                    ExternalUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    ItSystemId = table.Column<string>(type: "text", nullable: true),
                    PreviousName = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    BusinessTypeId = table.Column<int>(type: "integer", nullable: true),
                    Disabled = table.Column<bool>(type: "boolean", nullable: false),
                    ReferenceId = table.Column<int>(type: "integer", nullable: true),
                    ArchiveDuty = table.Column<int>(type: "integer", nullable: true),
                    ArchiveDutyComment = table.Column<string>(type: "text", nullable: true),
                    LegalName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LegalDataProcessorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SensitivePersonalDataTypeId = table.Column<int>(type: "integer", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    AccessModifier = table.Column<int>(type: "integer", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystem_BusinessTypes_BusinessTypeId",
                        column: x => x.BusinessTypeId,
                        principalTable: "BusinessTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItSystem_ExternalReferences_ReferenceId",
                        column: x => x.ReferenceId,
                        principalTable: "ExternalReferences",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItSystem_ItSystem_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItSystem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystem_Organization_BelongsToId",
                        column: x => x.BelongsToId,
                        principalTable: "Organization",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItSystem_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystem_SensitivePersonalDataTypes_SensitivePersonalDataTypeId",
                        column: x => x.SensitivePersonalDataTypeId,
                        principalTable: "SensitivePersonalDataTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItSystem_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystem_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItContractAgreementElementTypes",
                columns: table => new
                {
                    ItContract_Id = table.Column<int>(type: "integer", nullable: false),
                    AgreementElementType_Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItContractAgreementElementTypes", x => new { x.AgreementElementType_Id, x.ItContract_Id });
                    table.ForeignKey(
                        name: "FK_ItContractAgreementElementTypes_AgreementElementTypes_AgreementElementType_Id",
                        column: x => x.AgreementElementType_Id,
                        principalTable: "AgreementElementTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItContractAgreementElementTypes_ItContract_ItContract_Id",
                        column: x => x.ItContract_Id,
                        principalTable: "ItContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItContractOverviewReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    SourceEntityId = table.Column<int>(type: "integer", nullable: false),
                    SourceEntityUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ContractId = table.Column<string>(type: "text", nullable: true),
                    ParentContractId = table.Column<int>(type: "integer", nullable: true),
                    ParentContractName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ParentContractUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    CriticalityId = table.Column<int>(type: "integer", nullable: true),
                    CriticalityUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    CriticalityName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ResponsibleOrgUnitId = table.Column<int>(type: "integer", nullable: true),
                    ResponsibleOrgUnitName = table.Column<string>(type: "text", nullable: true),
                    SupplierId = table.Column<int>(type: "integer", nullable: true),
                    SupplierName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContractSigner = table.Column<string>(type: "text", nullable: true),
                    ContractTypeId = table.Column<int>(type: "integer", nullable: true),
                    ContractTypeUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractTypeName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ContractTemplateId = table.Column<int>(type: "integer", nullable: true),
                    ContractTemplateUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractTemplateName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PurchaseFormId = table.Column<int>(type: "integer", nullable: true),
                    PurchaseFormUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    PurchaseFormName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ProcurementStrategyId = table.Column<int>(type: "integer", nullable: true),
                    ProcurementStrategyUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcurementStrategyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ProcurementPlanYear = table.Column<int>(type: "integer", nullable: true),
                    ProcurementPlanQuarter = table.Column<int>(type: "integer", nullable: true),
                    ProcurementInitiated = table.Column<int>(type: "integer", nullable: true),
                    DataProcessingAgreementsCsv = table.Column<string>(type: "text", nullable: true),
                    ItSystemUsagesCsv = table.Column<string>(type: "text", nullable: true),
                    ItSystemUsagesSystemUuidCsv = table.Column<string>(type: "text", nullable: true),
                    NumberOfAssociatedSystemRelations = table.Column<int>(type: "integer", nullable: false),
                    ActiveReferenceTitle = table.Column<string>(type: "text", nullable: true),
                    ActiveReferenceUrl = table.Column<string>(type: "text", nullable: true),
                    ActiveReferenceExternalReferenceId = table.Column<string>(type: "text", nullable: true),
                    AccumulatedAcquisitionCost = table.Column<int>(type: "integer", nullable: false),
                    AccumulatedOperationCost = table.Column<int>(type: "integer", nullable: false),
                    AccumulatedOtherCost = table.Column<int>(type: "integer", nullable: false),
                    OperationRemunerationBegunDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PaymentModelId = table.Column<int>(type: "integer", nullable: true),
                    PaymentModelUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentModelName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PaymentFrequencyId = table.Column<int>(type: "integer", nullable: true),
                    PaymentFrequencyUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentFrequencyName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LatestAuditDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    AuditStatusWhite = table.Column<int>(type: "integer", nullable: false),
                    AuditStatusRed = table.Column<int>(type: "integer", nullable: false),
                    AuditStatusYellow = table.Column<int>(type: "integer", nullable: false),
                    AuditStatusGreen = table.Column<int>(type: "integer", nullable: false),
                    Duration = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OptionExtendId = table.Column<int>(type: "integer", nullable: true),
                    OptionExtendUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    OptionExtendName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    TerminationDeadlineId = table.Column<int>(type: "integer", nullable: true),
                    TerminationDeadlineUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    TerminationDeadlineName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    IrrevocableTo = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    TerminatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    LastEditedByUserId = table.Column<int>(type: "integer", nullable: true),
                    LastEditedByUserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastEditedAtDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Concluded = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItContractOverviewReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItContractOverviewReadModels_ItContract_SourceEntityId",
                        column: x => x.SourceEntityId,
                        principalTable: "ItContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItContractOverviewReadModels_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItContractRights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ObjectId = table.Column<int>(type: "integer", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItContractRights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItContractRights_ItContractRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "ItContractRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItContractRights_ItContract_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "ItContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItContractRights_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItContractRights_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItContractRights_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskRefItSystems",
                columns: table => new
                {
                    ItSystem_Id = table.Column<int>(type: "integer", nullable: false),
                    TaskRef_Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemTaskRef", x => new { x.ItSystem_Id, x.TaskRef_Id });
                    table.ForeignKey(
                        name: "FK_ItSystemTaskRef_ItSystem_ItSystem_Id",
                        column: x => x.ItSystem_Id,
                        principalTable: "ItSystem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItSystemTaskRef_TaskRef_TaskRef_Id",
                        column: x => x.TaskRef_Id,
                        principalTable: "TaskRef",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemUsage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Concluded = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Note = table.Column<string>(type: "text", nullable: true),
                    LocalSystemId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LocalCallName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LifeCycleStatus = table.Column<int>(type: "integer", nullable: true),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    ItSystemId = table.Column<int>(type: "integer", nullable: false),
                    ArchiveTypeId = table.Column<int>(type: "integer", nullable: true),
                    SensitiveDataTypeId = table.Column<int>(type: "integer", nullable: true),
                    ReferenceId = table.Column<int>(type: "integer", nullable: true),
                    ArchiveDuty = table.Column<int>(type: "integer", nullable: true),
                    ArchiveNotes = table.Column<string>(type: "text", nullable: true),
                    ArchiveFreq = table.Column<int>(type: "integer", nullable: true),
                    Registertype = table.Column<bool>(type: "boolean", nullable: true),
                    ArchiveSupplierId = table.Column<int>(type: "integer", nullable: true),
                    ArchiveLocationId = table.Column<int>(type: "integer", nullable: true),
                    ArchiveTestLocationId = table.Column<int>(type: "integer", nullable: true),
                    ItSystemCategoriesId = table.Column<int>(type: "integer", nullable: true),
                    GdprCriticality = table.Column<int>(type: "integer", nullable: true),
                    UserCount = table.Column<int>(type: "integer", nullable: true),
                    ContainsAITechnology = table.Column<int>(type: "integer", nullable: true),
                    GeneralPurpose = table.Column<string>(type: "text", nullable: true),
                    isBusinessCritical = table.Column<int>(type: "integer", nullable: true),
                    LinkToDirectoryUrl = table.Column<string>(type: "text", nullable: true),
                    LinkToDirectoryUrlName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    precautions = table.Column<int>(type: "integer", nullable: true),
                    precautionsOptionsEncryption = table.Column<bool>(type: "boolean", nullable: false),
                    precautionsOptionsPseudonomisering = table.Column<bool>(type: "boolean", nullable: false),
                    precautionsOptionsAccessControl = table.Column<bool>(type: "boolean", nullable: false),
                    precautionsOptionsLogning = table.Column<bool>(type: "boolean", nullable: false),
                    TechnicalSupervisionDocumentationUrlName = table.Column<string>(type: "text", nullable: true),
                    TechnicalSupervisionDocumentationUrl = table.Column<string>(type: "text", nullable: true),
                    UserSupervision = table.Column<int>(type: "integer", nullable: true),
                    UserSupervisionDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UserSupervisionDocumentationUrlName = table.Column<string>(type: "text", nullable: true),
                    UserSupervisionDocumentationUrl = table.Column<string>(type: "text", nullable: true),
                    riskAssessment = table.Column<int>(type: "integer", nullable: true),
                    riskAssesmentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    preriskAssessment = table.Column<int>(type: "integer", nullable: true),
                    PlannedRiskAssessmentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    RiskSupervisionDocumentationUrlName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    RiskSupervisionDocumentationUrl = table.Column<string>(type: "text", nullable: true),
                    noteRisks = table.Column<string>(type: "text", nullable: true),
                    DPIA = table.Column<int>(type: "integer", nullable: true),
                    DPIADateFor = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DPIASupervisionDocumentationUrlName = table.Column<string>(type: "text", nullable: true),
                    DPIASupervisionDocumentationUrl = table.Column<string>(type: "text", nullable: true),
                    answeringDataDPIA = table.Column<int>(type: "integer", nullable: true),
                    DPIAdeleteDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    numberDPIA = table.Column<int>(type: "integer", nullable: false),
                    HostedAt = table.Column<int>(type: "integer", nullable: true),
                    WebAccessibilityCompliance = table.Column<int>(type: "integer", nullable: true),
                    LastWebAccessibilityCheck = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    WebAccessibilityNotes = table.Column<string>(type: "text", nullable: true),
                    ArchiveFromSystem = table.Column<bool>(type: "boolean", nullable: true),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    RegisterTypeId = table.Column<int>(type: "integer", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsage_ArchiveLocation_ArchiveLocationId",
                        column: x => x.ArchiveLocationId,
                        principalTable: "ArchiveLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItSystemUsage_ArchiveTestLocation_ArchiveTestLocationId",
                        column: x => x.ArchiveTestLocationId,
                        principalTable: "ArchiveTestLocations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItSystemUsage_ArchiveTypes_ArchiveTypeId",
                        column: x => x.ArchiveTypeId,
                        principalTable: "ArchiveTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItSystemUsage_ExternalReferences_ReferenceId",
                        column: x => x.ReferenceId,
                        principalTable: "ExternalReferences",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItSystemUsage_ItSystemCategories_ItSystemCategoriesId",
                        column: x => x.ItSystemCategoriesId,
                        principalTable: "ItSystemCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItSystemUsage_ItSystem_ItSystemId",
                        column: x => x.ItSystemId,
                        principalTable: "ItSystem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystemUsage_Organization_ArchiveSupplierId",
                        column: x => x.ArchiveSupplierId,
                        principalTable: "Organization",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItSystemUsage_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystemUsage_RegisterTypes_RegisterTypeId",
                        column: x => x.RegisterTypeId,
                        principalTable: "RegisterTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItSystemUsage_SensitiveDataTypes_SensitiveDataTypeId",
                        column: x => x.SensitiveDataTypeId,
                        principalTable: "SensitiveDataTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ItSystemUsage_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystemUsage_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItContractOverviewReadModelDataProcessingAgreements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    DataProcessingRegistrationId = table.Column<int>(type: "integer", nullable: false),
                    DataProcessingRegistrationUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    DataProcessingRegistrationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItContractOverviewReadModelDataProcessingAgreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItContractOverviewReadModelDataProcessingAgreements_ItContractOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItContractOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItContractOverviewReadModelItSystemUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ItSystemUsageId = table.Column<int>(type: "integer", nullable: false),
                    ItSystemUsageUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ItSystemUsageSystemUuid = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ItSystemUsageName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ItSystemIsDisabled = table.Column<bool>(type: "boolean", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItContractOverviewReadModelItSystemUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItContractOverviewReadModelItSystemUsages_ItContractOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItContractOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItContractOverviewReadModelSystemRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    RelationId = table.Column<int>(type: "integer", nullable: false),
                    FromSystemUsageId = table.Column<int>(type: "integer", nullable: false),
                    ToSystemUsageId = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItContractOverviewReadModelSystemRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItContractOverviewReadModelSystemRelations_ItContractOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItContractOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItContractOverviewRoleAssignmentReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    UserFullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItContractOverviewRoleAssignmentReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItContractOverviewRoleAssignmentReadModels_ItContractOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItContractOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItContractItSystemUsages",
                columns: table => new
                {
                    ItContractId = table.Column<int>(type: "integer", nullable: false),
                    ItSystemUsageId = table.Column<int>(type: "integer", nullable: false),
                    ItSystemUsage_Id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItContractItSystemUsages", x => new { x.ItContractId, x.ItSystemUsageId });
                    table.ForeignKey(
                        name: "FK_ItContractItSystemUsages_ItContract_ItContractId",
                        column: x => x.ItContractId,
                        principalTable: "ItContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItContractItSystemUsages_ItSystemUsage_ItSystemUsageId",
                        column: x => x.ItSystemUsageId,
                        principalTable: "ItSystemUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItContractItSystemUsages_ItSystemUsage_ItSystemUsage_Id",
                        column: x => x.ItSystemUsage_Id,
                        principalTable: "ItSystemUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemRights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    ObjectId = table.Column<int>(type: "integer", nullable: false),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemRights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemRights_ItSystemRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "ItSystemRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystemRights_ItSystemUsage_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "ItSystemUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItSystemRights_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystemRights_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystemRights_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemUsageOrgUnitUsages",
                columns: table => new
                {
                    ItSystemUsageId = table.Column<int>(type: "integer", nullable: false),
                    OrganizationUnitId = table.Column<int>(type: "integer", nullable: false),
                    ResponsibleItSystemUsage_Id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageOrgUnitUsages", x => new { x.ItSystemUsageId, x.OrganizationUnitId });
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOrgUnitUsages_ItSystemUsage_ItSystemUsageId",
                        column: x => x.ItSystemUsageId,
                        principalTable: "ItSystemUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOrgUnitUsages_ItSystemUsage_ResponsibleItSystemUsage_Id",
                        column: x => x.ResponsibleItSystemUsage_Id,
                        principalTable: "ItSystemUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOrgUnitUsages_OrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalTable: "OrganizationUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemUsageOverviewReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    SourceEntityId = table.Column<int>(type: "integer", nullable: false),
                    SourceEntityUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalSystemUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    SystemName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SystemPreviousName = table.Column<string>(type: "text", nullable: true),
                    SystemDescription = table.Column<string>(type: "text", nullable: true),
                    ItSystemDisabled = table.Column<bool>(type: "boolean", nullable: false),
                    ActiveAccordingToValidityPeriod = table.Column<bool>(type: "boolean", nullable: false),
                    ActiveAccordingToLifeCycle = table.Column<bool>(type: "boolean", nullable: false),
                    SystemActive = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    ParentItSystemName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ParentItSystemId = table.Column<int>(type: "integer", nullable: true),
                    ParentItSystemUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    ParentItSystemDisabled = table.Column<bool>(type: "boolean", nullable: true),
                    ParentItSystemUsageUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    Version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContainsAITechnology = table.Column<int>(type: "integer", nullable: true),
                    LocalCallName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LocalSystemId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ItSystemUuid = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ResponsibleOrganizationUnitUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponsibleOrganizationUnitId = table.Column<int>(type: "integer", nullable: true),
                    ResponsibleOrganizationUnitName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItSystemBusinessTypeUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    ItSystemBusinessTypeId = table.Column<int>(type: "integer", nullable: true),
                    ItSystemBusinessTypeName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ItSystemRightsHolderId = table.Column<int>(type: "integer", nullable: true),
                    ItSystemRightsHolderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItSystemCategoriesId = table.Column<int>(type: "integer", nullable: true),
                    ItSystemCategoriesUuid = table.Column<Guid>(type: "uuid", nullable: true),
                    ItSystemCategoriesName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ItSystemKLEIdsAsCsv = table.Column<string>(type: "text", nullable: true),
                    ItSystemKLENamesAsCsv = table.Column<string>(type: "text", nullable: true),
                    LocalReferenceDocumentId = table.Column<string>(type: "text", nullable: true),
                    LocalReferenceUrl = table.Column<string>(type: "text", nullable: true),
                    LocalReferenceTitle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: true),
                    ObjectOwnerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastChangedById = table.Column<int>(type: "integer", nullable: true),
                    LastChangedByName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastChangedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Concluded = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MainContractId = table.Column<int>(type: "integer", nullable: true),
                    MainContractSupplierId = table.Column<int>(type: "integer", nullable: true),
                    MainContractSupplierName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MainContractIsActive = table.Column<int>(type: "integer", nullable: false),
                    SensitiveDataLevelsAsCsv = table.Column<string>(type: "text", nullable: true),
                    RiskAssessmentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PlannedRiskAssessmentDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchiveDuty = table.Column<int>(type: "integer", nullable: true),
                    IsHoldingDocument = table.Column<bool>(type: "boolean", nullable: false),
                    RiskSupervisionDocumentationName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    RiskSupervisionDocumentationUrl = table.Column<string>(type: "text", nullable: true),
                    LinkToDirectoryName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    LinkToDirectoryUrl = table.Column<string>(type: "text", nullable: true),
                    LifeCycleStatus = table.Column<int>(type: "integer", nullable: true),
                    DataProcessingRegistrationsConcludedAsCsv = table.Column<string>(type: "text", nullable: true),
                    DataProcessingRegistrationNamesAsCsv = table.Column<string>(type: "text", nullable: true),
                    GeneralPurpose = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    HostedAt = table.Column<int>(type: "integer", nullable: false),
                    UserCount = table.Column<int>(type: "integer", nullable: false),
                    GdprCriticality = table.Column<int>(type: "integer", nullable: true),
                    DependsOnInterfacesNamesAsCsv = table.Column<string>(type: "text", nullable: true),
                    IncomingRelatedItSystemUsagesNamesAsCsv = table.Column<string>(type: "text", nullable: true),
                    OutgoingRelatedItSystemUsagesNamesAsCsv = table.Column<string>(type: "text", nullable: true),
                    RelevantOrganizationUnitNamesAsCsv = table.Column<string>(type: "text", nullable: true),
                    AssociatedContractsNamesCsv = table.Column<string>(type: "text", nullable: true),
                    DPIAConducted = table.Column<int>(type: "integer", nullable: true),
                    IsBusinessCritical = table.Column<int>(type: "integer", nullable: true),
                    CatalogArchiveDuty = table.Column<int>(type: "integer", nullable: true),
                    CatalogArchiveDutyComment = table.Column<string>(type: "text", nullable: true),
                    WebAccessibilityCompliance = table.Column<int>(type: "integer", nullable: true),
                    LastWebAccessibilityCheck = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    WebAccessibilityNotes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageOverviewReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOverviewReadModels_ItSystemUsage_SourceEntityId",
                        column: x => x.SourceEntityId,
                        principalTable: "ItSystemUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOverviewReadModels_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemUsagePersonalDatas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ItSystemUsageId = table.Column<int>(type: "integer", nullable: false),
                    PersonalData = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsagePersonalDataOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsagePersonalDataOptions_ItSystemUsage_ItSystemUsageId",
                        column: x => x.ItSystemUsageId,
                        principalTable: "ItSystemUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemUsageSensitiveDataLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ItSystemUsage_Id = table.Column<int>(type: "integer", nullable: false),
                    SensitivityDataLevel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageSensitiveDataLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageSensitiveDataLevels_ItSystemUsage_ItSystemUsage_Id",
                        column: x => x.ItSystemUsage_Id,
                        principalTable: "ItSystemUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskRefItSystemUsages",
                columns: table => new
                {
                    ItSystemUsage_Id = table.Column<int>(type: "integer", nullable: false),
                    TaskRef_Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageTaskRef", x => new { x.ItSystemUsage_Id, x.TaskRef_Id });
                    table.ForeignKey(
                        name: "FK_ItSystemUsageTaskRef_ItSystemUsage_ItSystemUsage_Id",
                        column: x => x.ItSystemUsage_Id,
                        principalTable: "ItSystemUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageTaskRef_TaskRef_TaskRef_Id",
                        column: x => x.TaskRef_Id,
                        principalTable: "TaskRef",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    FromSystemUsageId = table.Column<int>(type: "integer", nullable: false),
                    ToSystemUsageId = table.Column<int>(type: "integer", nullable: false),
                    RelationInterfaceId = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    UsageFrequencyId = table.Column<int>(type: "integer", nullable: true),
                    AssociatedContractId = table.Column<int>(type: "integer", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemRelations_ItContract_AssociatedContractId",
                        column: x => x.AssociatedContractId,
                        principalTable: "ItContract",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemRelations_ItInterface_RelationInterfaceId",
                        column: x => x.RelationInterfaceId,
                        principalTable: "ItInterface",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemRelations_ItSystemUsage_FromSystemUsageId",
                        column: x => x.FromSystemUsageId,
                        principalTable: "ItSystemUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SystemRelations_ItSystemUsage_ToSystemUsageId",
                        column: x => x.ToSystemUsageId,
                        principalTable: "ItSystemUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemRelations_RelationFrequencyTypes_UsageFrequencyId",
                        column: x => x.UsageFrequencyId,
                        principalTable: "RelationFrequencyTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemRelations_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemRelations_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TaskRefItSystemUsageOptOut",
                columns: table => new
                {
                    ItSystemUsage_Id = table.Column<int>(type: "integer", nullable: false),
                    TaskRef_Id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskRefItSystemUsageOptOut", x => new { x.ItSystemUsage_Id, x.TaskRef_Id });
                    table.ForeignKey(
                        name: "FK_TaskRefItSystemUsageOptOut_ItSystemUsage_ItSystemUsage_Id",
                        column: x => x.ItSystemUsage_Id,
                        principalTable: "ItSystemUsage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskRefItSystemUsageOptOut_TaskRef_TaskRef_Id",
                        column: x => x.TaskRef_Id,
                        principalTable: "TaskRef",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserNotifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NotificationMessage = table.Column<string>(type: "text", nullable: false),
                    NotificationType = table.Column<int>(type: "integer", nullable: false),
                    NotificationRecipientId = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    OrganizationId = table.Column<int>(type: "integer", nullable: false),
                    Itcontract_Id = table.Column<int>(type: "integer", nullable: true),
                    ItSystemUsage_Id = table.Column<int>(type: "integer", nullable: true),
                    DataProcessingRegistration_Id = table.Column<int>(type: "integer", nullable: true),
                    ObjectOwnerId = table.Column<int>(type: "integer", nullable: false),
                    LastChanged = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LastChangedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotifications_DataProcessingRegistrations_DataProcessingRegistration_Id",
                        column: x => x.DataProcessingRegistration_Id,
                        principalTable: "DataProcessingRegistrations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserNotifications_ItContract_Itcontract_Id",
                        column: x => x.Itcontract_Id,
                        principalTable: "ItContract",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserNotifications_ItSystemUsage_ItSystemUsage_Id",
                        column: x => x.ItSystemUsage_Id,
                        principalTable: "ItSystemUsage",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserNotifications_Organization_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organization",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserNotifications_User_LastChangedByUserId",
                        column: x => x.LastChangedByUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserNotifications_User_NotificationRecipientId",
                        column: x => x.NotificationRecipientId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserNotifications_User_ObjectOwnerId",
                        column: x => x.ObjectOwnerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemUsageOverviewArchivePeriodReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    StartDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageOverviewArchivePeriodReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOverviewArchivePeriodReadModels_ItSystemUsageOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItSystemUsageOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemUsageOverviewDataProcessingRegistrationReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    DataProcessingRegistrationUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    DataProcessingRegistrationId = table.Column<int>(type: "integer", nullable: false),
                    DataProcessingRegistrationName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsAgreementConcluded = table.Column<int>(type: "integer", nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageOverviewDataProcessingRegistrationReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOverviewDataProcessingRegistrationReadModels_ItSystemUsageOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItSystemUsageOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemUsageOverviewInterfaceReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    InterfaceUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    InterfaceId = table.Column<int>(type: "integer", nullable: false),
                    InterfaceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageOverviewInterfaceReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOverviewInterfaceReadModels_ItSystemUsageOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItSystemUsageOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemUsageOverviewItContractReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ItContractUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ItContractId = table.Column<int>(type: "integer", nullable: false),
                    ItContractName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageOverviewItContractReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOverviewItContractReadModels_ItSystemUsageOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItSystemUsageOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemUsageOverviewUsedBySystemUsageReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ItSystemUsageUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ItSystemUsageId = table.Column<int>(type: "integer", nullable: false),
                    ItSystemUsageName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageOverviewItSystemUsageReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOverviewItSystemUsageReadModels_ItSystemUsageOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItSystemUsageOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemUsageOverviewRelevantOrgUnitReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    OrganizationUnitUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationUnitId = table.Column<int>(type: "integer", nullable: false),
                    OrganizationUnitName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageOverviewRelevantOrgUnitReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOverviewRelevantOrgUnitReadModels_ItSystemUsageOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItSystemUsageOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemUsageOverviewRoleAssignmentReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    RoleUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    UserFullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageOverviewRoleAssignmentReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOverviewRoleAssignmentReadModels_ItSystemUsageOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItSystemUsageOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemUsageOverviewSensitiveDataLevelReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    SensitivityDataLevel = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageOverviewSensitiveDataLevelReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOverviewSensitiveDataLevelReadModels_ItSystemUsageOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItSystemUsageOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemUsageOverviewTaskRefReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    KLEId = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: true),
                    KLEName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageOverviewTaskRefReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOverviewTaskRefReadModels_ItSystemUsageOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItSystemUsageOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ItSystemUsageOverviewUsingSystemUsageReadModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                        ,
                    ItSystemUsageUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    ItSystemUsageId = table.Column<int>(type: "integer", nullable: false),
                    ItSystemUsageName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItSystemUsageOverviewUsingSystemUsageReadModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItSystemUsageOverviewUsingSystemUsageReadModels_ItSystemUsageOverviewReadModels_ParentId",
                        column: x => x.ParentId,
                        principalTable: "ItSystemUsageOverviewReadModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Advice_LastChangedByUserId",
                table: "Advice",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Advice_ObjectOwnerId",
                table: "Advice",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AdviceSent_AdviceId",
                table: "AdviceSents",
                column: "AdviceId");

            migrationBuilder.CreateIndex(
                name: "IX_AdviceSent_LastChangedByUserId",
                table: "AdviceSents",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdviceSent_ObjectOwnerId",
                table: "AdviceSents",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AdviceUserRelations_AdviceId",
                table: "AdviceUserRelations",
                column: "AdviceId");

            migrationBuilder.CreateIndex(
                name: "IX_AdviceUserRelations_DataProcessingRegistrationRoleId",
                table: "AdviceUserRelations",
                column: "DataProcessingRegistrationRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AdviceUserRelations_ItContractRoleId",
                table: "AdviceUserRelations",
                column: "ItContractRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AdviceUserRelations_ItSystemRoleId",
                table: "AdviceUserRelations",
                column: "ItSystemRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AdviceUserRelations_LastChangedByUserId",
                table: "AdviceUserRelations",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdviceUserRelations_ObjectOwnerId",
                table: "AdviceUserRelations",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AgreementElementTypes_LastChangedByUserId",
                table: "AgreementElementTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AgreementElementTypes_ObjectOwnerId",
                table: "AgreementElementTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "AgreementElementTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveLocation_LastChangedByUserId",
                table: "ArchiveLocations",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveLocation_ObjectOwnerId",
                table: "ArchiveLocations",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "ArchiveLocations",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchivePeriod_ItSystemUsageId",
                table: "ArchivePeriod",
                column: "ItSystemUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivePeriod_LastChangedByUserId",
                table: "ArchivePeriod",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchivePeriod_ObjectOwnerId",
                table: "ArchivePeriod",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_ArchivePeriod_Uuid",
                table: "ArchivePeriod",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveTestLocation_LastChangedByUserId",
                table: "ArchiveTestLocations",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveTestLocation_ObjectOwnerId",
                table: "ArchiveTestLocations",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveTypes_LastChangedByUserId",
                table: "ArchiveTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ArchiveTypes_ObjectOwnerId",
                table: "ArchiveTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "ArchiveTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttachedOptions_LastChangedByUserId",
                table: "AttachedOptions",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AttachedOptions_ObjectOwnerId",
                table: "AttachedOptions",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_ObjectId",
                table: "AttachedOptions",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "UX_ObjectType",
                table: "AttachedOptions",
                column: "ObjectType");

            migrationBuilder.CreateIndex(
                name: "UX_OptionId",
                table: "AttachedOptions",
                column: "OptionId");

            migrationBuilder.CreateIndex(
                name: "UX_OptionType",
                table: "AttachedOptions",
                column: "OptionType");

            migrationBuilder.CreateIndex(
                name: "IX_BrokenLinkInExternalReference_BrokenReferenceOrigin_Id",
                table: "BrokenLinkInExternalReferences",
                column: "BrokenReferenceOrigin_Id");

            migrationBuilder.CreateIndex(
                name: "IX_BrokenLinkInExternalReference_ParentReport_Id",
                table: "BrokenLinkInExternalReferences",
                column: "ParentReport_Id");

            migrationBuilder.CreateIndex(
                name: "IX_BrokenLinkInInterface_BrokenReferenceOrigin_Id",
                table: "BrokenLinkInInterfaces",
                column: "BrokenReferenceOrigin_Id");

            migrationBuilder.CreateIndex(
                name: "IX_BrokenLinkInInterface_ParentReport_Id",
                table: "BrokenLinkInInterfaces",
                column: "ParentReport_Id");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessTypes_LastChangedByUserId",
                table: "BusinessTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessTypes_ObjectOwnerId",
                table: "BusinessTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "BusinessTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Config_LastChangedByUserId",
                table: "Config",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Config_ObjectOwnerId",
                table: "Config",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPersons_LastChangedByUserId",
                table: "ContactPersons",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactPersons_ObjectOwnerId",
                table: "ContactPersons",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryCodes_LastChangedByUserId",
                table: "CountryCodes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CountryCodes_ObjectOwnerId",
                table: "CountryCodes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "CountryCodes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CriticalityTypes_LastChangedByUserId",
                table: "CriticalityTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CriticalityTypes_ObjectOwnerId",
                table: "CriticalityTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "CriticalityTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomizedUiNodes_LastChangedByUserId",
                table: "CustomizedUiNodes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomizedUiNodes_ModuleId",
                table: "CustomizedUiNodes",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomizedUiNodes_ObjectOwnerId",
                table: "CustomizedUiNodes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingBasisForTransferOptions_LastChangedByUserId",
                table: "DataProcessingBasisForTransferOptions",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingBasisForTransferOptions_ObjectOwnerId",
                table: "DataProcessingBasisForTransferOptions",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "DataProcessingBasisForTransferOptions",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingCountryOptionDataProcessingRegistration_DataProcessingCountryOption_Id",
                table: "DataProcessingRegistrationDataProcessingCountryOptions",
                column: "DataProcessingCountryOption_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingCountryOptions_LastChangedByUserId",
                table: "DataProcessingCountryOptions",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingCountryOptions_ObjectOwnerId",
                table: "DataProcessingCountryOptions",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "DataProcessingCountryOptions",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingDataResponsibleOptions_LastChangedByUserId",
                table: "DataProcessingDataResponsibleOptions",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingDataResponsibleOptions_ObjectOwnerId",
                table: "DataProcessingDataResponsibleOptions",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "DataProcessingDataResponsibleOptions",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingOversightOptionDataProcessingRegistration_DataProcessingOversightOption_Id",
                table: "DataProcessingRegistrationDataProcessingOversightOptions",
                column: "DataProcessingOversightOption_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingOversightOptions_LastChangedByUserId",
                table: "DataProcessingOversightOptions",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingOversightOptions_ObjectOwnerId",
                table: "DataProcessingOversightOptions",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "DataProcessingOversightOptions",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrationItContract_DataProcessingRegistration_Id",
                table: "ItContractDataProcessingRegistrations",
                column: "DataProcessingRegistration_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrationItSystemUsage_ItSystemUsage_Id",
                table: "DataProcessingRegistrationItSystemUsages",
                column: "ItSystemUsage_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrationOrganization_Organization_Id",
                table: "DataProcessingRegistrationOrganizations",
                column: "Organization_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrationOversightDates_ParentId",
                table: "DataProcessingRegistrationOversightDates",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "DataProcessingRegistrationReadModel_Index_LastChangedAt",
                table: "DataProcessingRegistrationReadModels",
                column: "LastChangedAt");

            migrationBuilder.CreateIndex(
                name: "DataProcessingRegistrationReadModel_Index_LastChangedById",
                table: "DataProcessingRegistrationReadModels",
                column: "LastChangedById");

            migrationBuilder.CreateIndex(
                name: "DataProcessingRegistrationReadModel_Index_LastChangedByName",
                table: "DataProcessingRegistrationReadModels",
                column: "LastChangedByName");

            migrationBuilder.CreateIndex(
                name: "DataProcessingRegistrationReadModel_Index_MainReferenceTitle",
                table: "DataProcessingRegistrationReadModels",
                column: "MainReferenceTitle");

            migrationBuilder.CreateIndex(
                name: "DataProcessingRegistrationReadModel_Index_Name",
                table: "DataProcessingRegistrationReadModels",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrationReadModels_OrganizationId",
                table: "DataProcessingRegistrationReadModels",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrationReadModels_SourceEntityId",
                table: "DataProcessingRegistrationReadModels",
                column: "SourceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_DPR_Concluded",
                table: "DataProcessingRegistrationReadModels",
                column: "IsAgreementConcluded");

            migrationBuilder.CreateIndex(
                name: "IX_DPR_DataResponsible",
                table: "DataProcessingRegistrationReadModels",
                column: "DataResponsible");

            migrationBuilder.CreateIndex(
                name: "IX_DPR_DataResponsibleUuid",
                table: "DataProcessingRegistrationReadModels",
                column: "DataResponsibleUuid");

            migrationBuilder.CreateIndex(
                name: "IX_DPR_IsActive",
                table: "DataProcessingRegistrationReadModels",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DPR_IsOversightCompleted",
                table: "DataProcessingRegistrationReadModels",
                column: "IsOversightCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_DPR_MainContractIsActive",
                table: "DataProcessingRegistrationReadModels",
                column: "ActiveAccordingToMainContract");

            migrationBuilder.CreateIndex(
                name: "IX_DPR_OversightInterval",
                table: "DataProcessingRegistrationReadModels",
                column: "OversightInterval");

            migrationBuilder.CreateIndex(
                name: "IX_DPR_OversightScheduledInspectionDate",
                table: "DataProcessingRegistrationReadModels",
                column: "OversightScheduledInspectionDate");

            migrationBuilder.CreateIndex(
                name: "IX_DPR_ResponsibleOrgUnitId",
                table: "DataProcessingRegistrationReadModels",
                column: "ResponsibleOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DPR_ResponsibleOrgUnitUuid",
                table: "DataProcessingRegistrationReadModels",
                column: "ResponsibleOrgUnitUuid");

            migrationBuilder.CreateIndex(
                name: "IX_DPR_TransferToInsecureThirdCountries",
                table: "DataProcessingRegistrationReadModels",
                column: "TransferToInsecureThirdCountries");

            migrationBuilder.CreateIndex(
                name: "IX_DRP_BasisForTransfer",
                table: "DataProcessingRegistrationReadModels",
                column: "BasisForTransfer");

            migrationBuilder.CreateIndex(
                name: "IX_DRP_BasisForTransferUuid",
                table: "DataProcessingRegistrationReadModels",
                column: "BasisForTransferUuid");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrationRights_LastChangedByUserId",
                table: "DataProcessingRegistrationRights",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrationRights_ObjectId",
                table: "DataProcessingRegistrationRights",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrationRights_ObjectOwnerId",
                table: "DataProcessingRegistrationRights",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrationRights_RoleId",
                table: "DataProcessingRegistrationRights",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrationRights_UserId",
                table: "DataProcessingRegistrationRights",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrationRoleAssignmentReadModels_ParentId",
                table: "DataProcessingRegistrationRoleAssignmentReadModels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleId",
                table: "DataProcessingRegistrationRoleAssignmentReadModels",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFullName",
                table: "DataProcessingRegistrationRoleAssignmentReadModels",
                column: "UserFullName");

            migrationBuilder.CreateIndex(
                name: "IX_UserId",
                table: "DataProcessingRegistrationRoleAssignmentReadModels",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrationRoles_LastChangedByUserId",
                table: "DataProcessingRegistrationRoles",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrationRoles_ObjectOwnerId",
                table: "DataProcessingRegistrationRoles",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "DataProcessingRegistrationRoles",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrations_BasisForTransferId",
                table: "DataProcessingRegistrations",
                column: "BasisForTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrations_DataResponsible_Id",
                table: "DataProcessingRegistrations",
                column: "DataResponsible_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrations_LastChangedByUserId",
                table: "DataProcessingRegistrations",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrations_MainContractId",
                table: "DataProcessingRegistrations",
                column: "MainContractId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrations_ObjectOwnerId",
                table: "DataProcessingRegistrations",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrations_ReferenceId",
                table: "DataProcessingRegistrations",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProcessingRegistrations_ResponsibleOrganizationUnitId",
                table: "DataProcessingRegistrations",
                column: "ResponsibleOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Name",
                table: "DataProcessingRegistrations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationId",
                table: "DataProcessingRegistrations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "UX_DataProcessingRegistration_Uuid",
                table: "DataProcessingRegistrations",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_NameUniqueToOrg",
                table: "DataProcessingRegistrations",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataProtectionAdvisors_Cvr",
                table: "DataProtectionAdvisors",
                column: "Cvr");

            migrationBuilder.CreateIndex(
                name: "IX_DataProtectionAdvisors_LastChangedByUserId",
                table: "DataProtectionAdvisors",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProtectionAdvisors_ObjectOwnerId",
                table: "DataProtectionAdvisors",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DataProtectionAdvisors_OrganizationId",
                table: "DataProtectionAdvisors",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DataResponsibles_Cvr",
                table: "DataResponsibles",
                column: "Cvr");

            migrationBuilder.CreateIndex(
                name: "IX_DataResponsibles_LastChangedByUserId",
                table: "DataResponsibles",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataResponsibles_ObjectOwnerId",
                table: "DataResponsibles",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DataResponsibles_OrganizationId",
                table: "DataResponsibles",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_DataRow_DataTypeId",
                table: "DataRow",
                column: "DataTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DataRow_ItInterfaceId",
                table: "DataRow",
                column: "ItInterfaceId");

            migrationBuilder.CreateIndex(
                name: "IX_DataRow_LastChangedByUserId",
                table: "DataRow",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataRow_ObjectOwnerId",
                table: "DataRow",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_uuid",
                table: "DataRow",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DataTypes_LastChangedByUserId",
                table: "DataTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DataTypes_ObjectOwnerId",
                table: "DataTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "DataTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EconomyStream_ExternPaymentForId",
                table: "EconomyStream",
                column: "ExternPaymentForId");

            migrationBuilder.CreateIndex(
                name: "IX_EconomyStream_InternPaymentForId",
                table: "EconomyStream",
                column: "InternPaymentForId");

            migrationBuilder.CreateIndex(
                name: "IX_EconomyStream_LastChangedByUserId",
                table: "EconomyStream",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EconomyStream_ObjectOwnerId",
                table: "EconomyStream",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_EconomyStream_OrganizationUnitId",
                table: "EconomyStream",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Exhibit_ItSystemId",
                table: "Exhibit",
                column: "ItSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_Exhibit_LastChangedByUserId",
                table: "Exhibit",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Exhibit_ObjectOwnerId",
                table: "Exhibit",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalReferences_DataProcessingRegistration_Id",
                table: "ExternalReferences",
                column: "DataProcessingRegistration_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalReferences_ItContract_Id",
                table: "ExternalReferences",
                column: "ItContract_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalReferences_ItSystem_Id",
                table: "ExternalReferences",
                column: "ItSystem_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalReferences_ItSystemUsage_Id",
                table: "ExternalReferences",
                column: "ItSystemUsage_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalReferences_LastChangedByUserId",
                table: "ExternalReferences",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalReferences_ObjectOwnerId",
                table: "ExternalReferences",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_ExternalReference_Uuid",
                table: "ExternalReferences",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HelpTexts_LastChangedByUserId",
                table: "HelpTexts",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpTexts_ObjectOwnerId",
                table: "HelpTexts",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_InterfaceTypes_LastChangedByUserId",
                table: "InterfaceTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InterfaceTypes_ObjectOwnerId",
                table: "InterfaceTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "InterfaceTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_ContractTemplateId",
                table: "ItContract",
                column: "ContractTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_ContractTypeId",
                table: "ItContract",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_CriticalityId",
                table: "ItContract",
                column: "CriticalityId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_LastChangedByUserId",
                table: "ItContract",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_ObjectOwnerId",
                table: "ItContract",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_OptionExtendId",
                table: "ItContract",
                column: "OptionExtendId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_ParentId",
                table: "ItContract",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_PaymentFreqencyId",
                table: "ItContract",
                column: "PaymentFreqencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_PaymentModelId",
                table: "ItContract",
                column: "PaymentModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_PriceRegulationId",
                table: "ItContract",
                column: "PriceRegulationId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_ProcurementStrategyId",
                table: "ItContract",
                column: "ProcurementStrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_PurchaseFormId",
                table: "ItContract",
                column: "PurchaseFormId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_ReferenceId",
                table: "ItContract",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_ResponsibleOrganizationUnitId",
                table: "ItContract",
                column: "ResponsibleOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_SupplierId",
                table: "ItContract",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_TerminationDeadlineId",
                table: "ItContract",
                column: "TerminationDeadlineId");

            migrationBuilder.CreateIndex(
                name: "IX_Name",
                table: "ItContract",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationId",
                table: "ItContract",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementInitiated",
                table: "ItContract",
                column: "ProcurementInitiated");

            migrationBuilder.CreateIndex(
                name: "UX_Contract_Uuid",
                table: "ItContract",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_NameUniqueToOrg",
                table: "ItContract",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItContractAgreementElementTypes_ItContract_Id",
                table: "ItContractAgreementElementTypes",
                column: "ItContract_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractItSystemUsages_ItSystemUsage_Id",
                table: "ItContractItSystemUsages",
                column: "ItSystemUsage_Id",
                unique: true,
                filter: "[ItSystemUsage_Id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractItSystemUsages_ItSystemUsageId",
                table: "ItContractItSystemUsages",
                column: "ItSystemUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Read_Dpr_Name",
                table: "ItContractOverviewReadModelDataProcessingAgreements",
                column: "DataProcessingRegistrationName");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Read_Dpr_Uuid",
                table: "ItContractOverviewReadModelDataProcessingAgreements",
                column: "DataProcessingRegistrationUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractOverviewReadModelDataProcessingAgreements_ParentId",
                table: "ItContractOverviewReadModelDataProcessingAgreements",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Read_System_Name",
                table: "ItContractOverviewReadModelItSystemUsages",
                column: "ItSystemUsageName");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Read_System_Usage_Uuid",
                table: "ItContractOverviewReadModelItSystemUsages",
                column: "ItSystemUsageUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Read_System_Uuid",
                table: "ItContractOverviewReadModelItSystemUsages",
                column: "ItSystemUsageSystemUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractOverviewReadModelItSystemUsages_ParentId",
                table: "ItContractOverviewReadModelItSystemUsages",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AccumulatedAcquisitionCost",
                table: "ItContractOverviewReadModels",
                column: "AccumulatedAcquisitionCost");

            migrationBuilder.CreateIndex(
                name: "IX_AccumulatedOperationCost",
                table: "ItContractOverviewReadModels",
                column: "AccumulatedOperationCost");

            migrationBuilder.CreateIndex(
                name: "IX_AccumulatedOtherCost",
                table: "ItContractOverviewReadModels",
                column: "AccumulatedOtherCost");

            migrationBuilder.CreateIndex(
                name: "IX_Concluded",
                table: "ItContractOverviewReadModels",
                column: "Concluded");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_Active",
                table: "ItContractOverviewReadModels",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_Name",
                table: "ItContractOverviewReadModels",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_CriticalityType_Id",
                table: "ItContractOverviewReadModels",
                column: "CriticalityId");

            migrationBuilder.CreateIndex(
                name: "IX_CriticalityType_Name",
                table: "ItContractOverviewReadModels",
                column: "CriticalityName");

            migrationBuilder.CreateIndex(
                name: "IX_CriticalityType_Uuid",
                table: "ItContractOverviewReadModels",
                column: "CriticalityUuid");

            migrationBuilder.CreateIndex(
                name: "IX_Duration",
                table: "ItContractOverviewReadModels",
                column: "Duration");

            migrationBuilder.CreateIndex(
                name: "IX_ExpirationDate",
                table: "ItContractOverviewReadModels",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_IrrevocableTo",
                table: "ItContractOverviewReadModels",
                column: "IrrevocableTo");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractOverviewReadModels_OrganizationId",
                table: "ItContractOverviewReadModels",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractOverviewReadModels_SourceEntityId",
                table: "ItContractOverviewReadModels",
                column: "SourceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractTemplateType_Id",
                table: "ItContractOverviewReadModels",
                column: "ContractTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractTemplateType_Name",
                table: "ItContractOverviewReadModels",
                column: "ContractTemplateName");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractTemplateType_Uuid",
                table: "ItContractOverviewReadModels",
                column: "ContractTemplateUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractType_Id",
                table: "ItContractOverviewReadModels",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractType_Name",
                table: "ItContractOverviewReadModels",
                column: "ContractTypeName");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractType_Uuid",
                table: "ItContractOverviewReadModels",
                column: "ContractTypeUuid");

            migrationBuilder.CreateIndex(
                name: "IX_LastEditedAtDate",
                table: "ItContractOverviewReadModels",
                column: "LastEditedAtDate");

            migrationBuilder.CreateIndex(
                name: "IX_LastEditedByUserId",
                table: "ItContractOverviewReadModels",
                column: "LastEditedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LastEditedByUserName",
                table: "ItContractOverviewReadModels",
                column: "LastEditedByUserName");

            migrationBuilder.CreateIndex(
                name: "IX_LatestAuditDate",
                table: "ItContractOverviewReadModels",
                column: "LatestAuditDate");

            migrationBuilder.CreateIndex(
                name: "IX_NumberOfAssociatedSystemRelations",
                table: "ItContractOverviewReadModels",
                column: "NumberOfAssociatedSystemRelations");

            migrationBuilder.CreateIndex(
                name: "IX_OperationRemunerationBegunDate",
                table: "ItContractOverviewReadModels",
                column: "OperationRemunerationBegunDate");

            migrationBuilder.CreateIndex(
                name: "IX_OptionExtendType_Id",
                table: "ItContractOverviewReadModels",
                column: "OptionExtendId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionExtendType_Name",
                table: "ItContractOverviewReadModels",
                column: "OptionExtendName");

            migrationBuilder.CreateIndex(
                name: "IX_OptionExtendType_Uuid",
                table: "ItContractOverviewReadModels",
                column: "OptionExtendUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ParentContract_Id",
                table: "ItContractOverviewReadModels",
                column: "ParentContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ParentContract_Name",
                table: "ItContractOverviewReadModels",
                column: "ParentContractName");

            migrationBuilder.CreateIndex(
                name: "IX_ParentContract_Uuid",
                table: "ItContractOverviewReadModels",
                column: "ParentContractUuid");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentFreqencyType_Id",
                table: "ItContractOverviewReadModels",
                column: "PaymentFrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentFreqencyType_Name",
                table: "ItContractOverviewReadModels",
                column: "PaymentFrequencyName");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentFreqencyType_Uuid",
                table: "ItContractOverviewReadModels",
                column: "PaymentFrequencyUuid");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentModelType_Id",
                table: "ItContractOverviewReadModels",
                column: "PaymentModelId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentModelType_Name",
                table: "ItContractOverviewReadModels",
                column: "PaymentModelName");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentModelType_Uuid",
                table: "ItContractOverviewReadModels",
                column: "PaymentModelUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementInitiated",
                table: "ItContractOverviewReadModels",
                column: "ProcurementInitiated");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementPlanQuarter",
                table: "ItContractOverviewReadModels",
                column: "ProcurementPlanQuarter");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementPlanYear",
                table: "ItContractOverviewReadModels",
                column: "ProcurementPlanYear");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementStrategyType_Id",
                table: "ItContractOverviewReadModels",
                column: "ProcurementStrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementStrategyType_Name",
                table: "ItContractOverviewReadModels",
                column: "ProcurementStrategyName");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementStrategyType_Uuid",
                table: "ItContractOverviewReadModels",
                column: "ProcurementStrategyUuid");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFormType_Id",
                table: "ItContractOverviewReadModels",
                column: "PurchaseFormId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFormType_Name",
                table: "ItContractOverviewReadModels",
                column: "PurchaseFormName");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFormType_Uuid",
                table: "ItContractOverviewReadModels",
                column: "PurchaseFormUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ResponsibleOrgUnitId",
                table: "ItContractOverviewReadModels",
                column: "ResponsibleOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierId",
                table: "ItContractOverviewReadModels",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierName",
                table: "ItContractOverviewReadModels",
                column: "SupplierName");

            migrationBuilder.CreateIndex(
                name: "IX_TerminatedAt",
                table: "ItContractOverviewReadModels",
                column: "TerminatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TerminationDeadlineType_Id",
                table: "ItContractOverviewReadModels",
                column: "TerminationDeadlineId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminationDeadlineType_Name",
                table: "ItContractOverviewReadModels",
                column: "TerminationDeadlineName");

            migrationBuilder.CreateIndex(
                name: "IX_TerminationDeadlineType_Uuid",
                table: "ItContractOverviewReadModels",
                column: "TerminationDeadlineUuid");

            migrationBuilder.CreateIndex(
                name: "IX_FromSystemUsageId",
                table: "ItContractOverviewReadModelSystemRelations",
                column: "FromSystemUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractOverviewReadModelSystemRelations_ParentId",
                table: "ItContractOverviewReadModelSystemRelations",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_RelationId",
                table: "ItContractOverviewReadModelSystemRelations",
                column: "RelationId");

            migrationBuilder.CreateIndex(
                name: "IX_ToSystemUsageId",
                table: "ItContractOverviewReadModelSystemRelations",
                column: "ToSystemUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Read_Role_Id",
                table: "ItContractOverviewRoleAssignmentReadModels",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Read_User_Id",
                table: "ItContractOverviewRoleAssignmentReadModels",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContract_Read_User_Name",
                table: "ItContractOverviewRoleAssignmentReadModels",
                column: "UserFullName");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractOverviewRoleAssignmentReadModels_ParentId",
                table: "ItContractOverviewRoleAssignmentReadModels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractRights_LastChangedByUserId",
                table: "ItContractRights",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractRights_ObjectId",
                table: "ItContractRights",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractRights_ObjectOwnerId",
                table: "ItContractRights",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractRights_RoleId",
                table: "ItContractRights",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractRights_UserId",
                table: "ItContractRights",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractRoles_LastChangedByUserId",
                table: "ItContractRoles",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractRoles_ObjectOwnerId",
                table: "ItContractRoles",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "ItContractRoles",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItContractTemplateTypes_LastChangedByUserId",
                table: "ItContractTemplateTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractTemplateTypes_ObjectOwnerId",
                table: "ItContractTemplateTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "ItContractTemplateTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItContractTypes_LastChangedByUserId",
                table: "ItContractTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItContractTypes_ObjectOwnerId",
                table: "ItContractTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "ItContractTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItInterface_InterfaceId",
                table: "ItInterface",
                column: "InterfaceId");

            migrationBuilder.CreateIndex(
                name: "IX_ItInterface_LastChangedByUserId",
                table: "ItInterface",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItInterface_ObjectOwnerId",
                table: "ItInterface",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Name",
                table: "ItInterface",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationId",
                table: "ItInterface",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_Version",
                table: "ItInterface",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "UX_AccessModifier",
                table: "ItInterface",
                column: "AccessModifier");

            migrationBuilder.CreateIndex(
                name: "UX_ItInterface_Uuid",
                table: "ItInterface",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_NameAndVersionUniqueToOrg",
                table: "ItInterface",
                columns: new[] { "OrganizationId", "Name", "ItInterfaceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ItSystem_IX_LegalDataProcessorName",
                table: "ItSystem",
                column: "LegalDataProcessorName");

            migrationBuilder.CreateIndex(
                name: "ItSystem_IX_LegalName",
                table: "ItSystem",
                column: "LegalName");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystem_BelongsToId",
                table: "ItSystem",
                column: "BelongsToId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystem_BusinessTypeId",
                table: "ItSystem",
                column: "BusinessTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystem_LastChangedByUserId",
                table: "ItSystem",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystem_ObjectOwnerId",
                table: "ItSystem",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystem_ParentId",
                table: "ItSystem",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystem_ReferenceId",
                table: "ItSystem",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystem_SensitivePersonalDataTypeId",
                table: "ItSystem",
                column: "SensitivePersonalDataTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Name",
                table: "ItSystem",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationId",
                table: "ItSystem",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "UX_AccessModifier",
                table: "ItSystem",
                column: "AccessModifier");

            migrationBuilder.CreateIndex(
                name: "UX_NameUniqueToOrg",
                table: "ItSystem",
                columns: new[] { "OrganizationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_System_Uuuid",
                table: "ItSystem",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemCategories_LastChangedByUserId",
                table: "ItSystemCategories",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemCategories_ObjectOwnerId",
                table: "ItSystemCategories",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "ItSystemCategories",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemRights_LastChangedByUserId",
                table: "ItSystemRights",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemRights_ObjectId",
                table: "ItSystemRights",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemRights_ObjectOwnerId",
                table: "ItSystemRights",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemRights_RoleId",
                table: "ItSystemRights",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemRights_UserId",
                table: "ItSystemRights",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemRoles_LastChangedByUserId",
                table: "ItSystemRoles",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemRoles_ObjectOwnerId",
                table: "ItSystemRoles",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "ItSystemRoles",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemTaskRef_TaskRef_Id",
                table: "TaskRefItSystems",
                column: "TaskRef_Id");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsage_Index_GdprCriticality",
                table: "ItSystemUsage",
                column: "GdprCriticality");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsage_Index_LifeCycleStatus",
                table: "ItSystemUsage",
                column: "LifeCycleStatus");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsage_Index_LinkToDirectoryUrlName",
                table: "ItSystemUsage",
                column: "LinkToDirectoryUrlName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsage_Index_LocalCallName",
                table: "ItSystemUsage",
                column: "LocalCallName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsage_Index_LocalSystemId",
                table: "ItSystemUsage",
                column: "LocalSystemId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsage_Index_RiskSupervisionDocumentationUrlName",
                table: "ItSystemUsage",
                column: "RiskSupervisionDocumentationUrlName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsage_Index_Version",
                table: "ItSystemUsage",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsage_ArchiveLocationId",
                table: "ItSystemUsage",
                column: "ArchiveLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsage_ArchiveSupplierId",
                table: "ItSystemUsage",
                column: "ArchiveSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsage_ArchiveTestLocationId",
                table: "ItSystemUsage",
                column: "ArchiveTestLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsage_ArchiveTypeId",
                table: "ItSystemUsage",
                column: "ArchiveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsage_ItSystemCategoriesId",
                table: "ItSystemUsage",
                column: "ItSystemCategoriesId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsage_ItSystemId",
                table: "ItSystemUsage",
                column: "ItSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsage_LastChangedByUserId",
                table: "ItSystemUsage",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsage_ObjectOwnerId",
                table: "ItSystemUsage",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsage_OrganizationId",
                table: "ItSystemUsage",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsage_ReferenceId",
                table: "ItSystemUsage",
                column: "ReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsage_RegisterTypeId",
                table: "ItSystemUsage",
                column: "RegisterTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsage_SensitiveDataTypeId",
                table: "ItSystemUsage",
                column: "SensitiveDataTypeId");

            migrationBuilder.CreateIndex(
                name: "UX_ItSystemUsage_Uuid",
                table: "ItSystemUsage",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOrgUnitUsages_OrganizationUnitId",
                table: "ItSystemUsageOrgUnitUsages",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOrgUnitUsages_ResponsibleItSystemUsage_Id",
                table: "ItSystemUsageOrgUnitUsages",
                column: "ResponsibleItSystemUsage_Id",
                unique: true,
                filter: "[ResponsibleItSystemUsage_Id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewArchivePeriodReadModel_index_EndDate",
                table: "ItSystemUsageOverviewArchivePeriodReadModels",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewArchivePeriodReadModel_index_StartDate",
                table: "ItSystemUsageOverviewArchivePeriodReadModels",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOverviewArchivePeriodReadModels_ParentId",
                table: "ItSystemUsageOverviewArchivePeriodReadModels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewArchivePeriodReadModel_index_DataProcessingRegistrationId",
                table: "ItSystemUsageOverviewDataProcessingRegistrationReadModels",
                column: "DataProcessingRegistrationId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewArchivePeriodReadModel_index_DataProcessingRegistrationName",
                table: "ItSystemUsageOverviewDataProcessingRegistrationReadModels",
                column: "DataProcessingRegistrationName");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOverviewDataProcessingRegistrationReadModels_ParentId",
                table: "ItSystemUsageOverviewDataProcessingRegistrationReadModels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewInterfaceReadModel_index_InterfaceId",
                table: "ItSystemUsageOverviewInterfaceReadModels",
                column: "InterfaceId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewInterfaceReadModel_index_InterfaceName",
                table: "ItSystemUsageOverviewInterfaceReadModels",
                column: "InterfaceName");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOverviewInterfaceReadModels_ParentId",
                table: "ItSystemUsageOverviewInterfaceReadModels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "ItContractId",
                table: "ItSystemUsageOverviewItContractReadModels",
                column: "ItContractId");

            migrationBuilder.CreateIndex(
                name: "ItContractNameName",
                table: "ItSystemUsageOverviewItContractReadModels",
                column: "ItContractName");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOverviewItContractReadModels_ParentId",
                table: "ItSystemUsageOverviewItContractReadModels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewItSystemUsageReadModel_index_ItSystemUsageId",
                table: "ItSystemUsageOverviewUsedBySystemUsageReadModels",
                column: "ItSystemUsageId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewItSystemUsageReadModel_index_ItSystemUsageName",
                table: "ItSystemUsageOverviewUsedBySystemUsageReadModels",
                column: "ItSystemUsageName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewItSystemUsageReadModel_index_ItSystemUsageUuid",
                table: "ItSystemUsageOverviewUsedBySystemUsageReadModels",
                column: "ItSystemUsageUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOverviewItSystemUsageReadModels_ParentId",
                table: "ItSystemUsageOverviewUsedBySystemUsageReadModels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ActiveAccordingToLifeCycle",
                table: "ItSystemUsageOverviewReadModels",
                column: "ActiveAccordingToLifeCycle");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ActiveAccordingToValidityPeriod",
                table: "ItSystemUsageOverviewReadModels",
                column: "ActiveAccordingToValidityPeriod");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ArchiveDuty",
                table: "ItSystemUsageOverviewReadModels",
                column: "ArchiveDuty");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_CatalogArchiveDuty",
                table: "ItSystemUsageOverviewReadModels",
                column: "CatalogArchiveDuty");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_DPIAConducted",
                table: "ItSystemUsageOverviewReadModels",
                column: "DPIAConducted");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_GdprCriticality",
                table: "ItSystemUsageOverviewReadModels",
                column: "GdprCriticality");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_GeneralPurpose",
                table: "ItSystemUsageOverviewReadModels",
                column: "GeneralPurpose");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_HostedAt",
                table: "ItSystemUsageOverviewReadModels",
                column: "HostedAt");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_IsBusinessCritical",
                table: "ItSystemUsageOverviewReadModels",
                column: "IsBusinessCritical");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_IsHoldingDocument",
                table: "ItSystemUsageOverviewReadModels",
                column: "IsHoldingDocument");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ItSystemBelongsToId",
                table: "ItSystemUsageOverviewReadModels",
                column: "ItSystemRightsHolderId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ItSystemBelongsToName",
                table: "ItSystemUsageOverviewReadModels",
                column: "ItSystemRightsHolderName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ItSystemBusinessTypeId",
                table: "ItSystemUsageOverviewReadModels",
                column: "ItSystemBusinessTypeId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ItSystemBusinessTypeName",
                table: "ItSystemUsageOverviewReadModels",
                column: "ItSystemBusinessTypeName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ItSystemBusinessTypeUuid",
                table: "ItSystemUsageOverviewReadModels",
                column: "ItSystemBusinessTypeUuid");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ItSystemCategoriesId",
                table: "ItSystemUsageOverviewReadModels",
                column: "ItSystemCategoriesId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ItSystemCategoriesName",
                table: "ItSystemUsageOverviewReadModels",
                column: "ItSystemCategoriesName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ItSystemCategoriesUuid",
                table: "ItSystemUsageOverviewReadModels",
                column: "ItSystemCategoriesUuid");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ItSystemDisabled",
                table: "ItSystemUsageOverviewReadModels",
                column: "ItSystemDisabled");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ItSystemParentName",
                table: "ItSystemUsageOverviewReadModels",
                column: "ParentItSystemName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ItSystemUuid",
                table: "ItSystemUsageOverviewReadModels",
                column: "ItSystemUuid");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_LastChangedById",
                table: "ItSystemUsageOverviewReadModels",
                column: "LastChangedById");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_LastChangedByName",
                table: "ItSystemUsageOverviewReadModels",
                column: "LastChangedByName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_LifeCycleStatus",
                table: "ItSystemUsageOverviewReadModels",
                column: "LifeCycleStatus");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_LinkToDirectoryName",
                table: "ItSystemUsageOverviewReadModels",
                column: "LinkToDirectoryName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_LocalCallName",
                table: "ItSystemUsageOverviewReadModels",
                column: "LocalCallName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_LocalReferenceTitle",
                table: "ItSystemUsageOverviewReadModels",
                column: "LocalReferenceTitle");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_LocalSystemId",
                table: "ItSystemUsageOverviewReadModels",
                column: "LocalSystemId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_MainContractId",
                table: "ItSystemUsageOverviewReadModels",
                column: "MainContractId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_MainContractSupplierId",
                table: "ItSystemUsageOverviewReadModels",
                column: "MainContractSupplierId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_MainContractSupplierName",
                table: "ItSystemUsageOverviewReadModels",
                column: "MainContractSupplierName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_Name",
                table: "ItSystemUsageOverviewReadModels",
                column: "SystemName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ObjectOwnerId",
                table: "ItSystemUsageOverviewReadModels",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ObjectOwnerName",
                table: "ItSystemUsageOverviewReadModels",
                column: "ObjectOwnerName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ParentItSystemUsageUuid",
                table: "ItSystemUsageOverviewReadModels",
                column: "ParentItSystemUsageUuid");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationId",
                table: "ItSystemUsageOverviewReadModels",
                column: "ResponsibleOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationName",
                table: "ItSystemUsageOverviewReadModels",
                column: "ResponsibleOrganizationUnitName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationUuid",
                table: "ItSystemUsageOverviewReadModels",
                column: "ResponsibleOrganizationUnitUuid");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_RiskSupervisionDocumentationName",
                table: "ItSystemUsageOverviewReadModels",
                column: "RiskSupervisionDocumentationName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_SystemActive",
                table: "ItSystemUsageOverviewReadModels",
                column: "SystemActive");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_UserCount",
                table: "ItSystemUsageOverviewReadModels",
                column: "UserCount");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewReadModel_Index_Version",
                table: "ItSystemUsageOverviewReadModels",
                column: "Version");

            migrationBuilder.CreateIndex(
                name: "IX_Concluded",
                table: "ItSystemUsageOverviewReadModels",
                column: "Concluded");

            migrationBuilder.CreateIndex(
                name: "IX_ExpirationDate",
                table: "ItSystemUsageOverviewReadModels",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOverviewReadModels_OrganizationId",
                table: "ItSystemUsageOverviewReadModels",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOverviewReadModels_SourceEntityId",
                table: "ItSystemUsageOverviewReadModels",
                column: "SourceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_LastChangedAt",
                table: "ItSystemUsageOverviewReadModels",
                column: "LastChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LastWebAccessibilityCheck",
                table: "ItSystemUsageOverviewReadModels",
                column: "LastWebAccessibilityCheck");

            migrationBuilder.CreateIndex(
                name: "IX_PlannedRiskAssessmentDate",
                table: "ItSystemUsageOverviewReadModels",
                column: "PlannedRiskAssessmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAssessmentDate",
                table: "ItSystemUsageOverviewReadModels",
                column: "RiskAssessmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_WebAccessibilityCompliance",
                table: "ItSystemUsageOverviewReadModels",
                column: "WebAccessibilityCompliance");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOverviewRelevantOrgUnitReadModels_ParentId",
                table: "ItSystemUsageOverviewRelevantOrgUnitReadModels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Name",
                table: "ItSystemUsageOverviewRelevantOrgUnitReadModels",
                column: "OrganizationUnitName");

            migrationBuilder.CreateIndex(
                name: "IX_OrgUnitId",
                table: "ItSystemUsageOverviewRelevantOrgUnitReadModels",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_OrgUnitUuid",
                table: "ItSystemUsageOverviewRelevantOrgUnitReadModels",
                column: "OrganizationUnitUuid");

            migrationBuilder.CreateIndex(
                name: "IX_Email",
                table: "ItSystemUsageOverviewRoleAssignmentReadModels",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOverviewRoleAssignmentReadModels_ParentId",
                table: "ItSystemUsageOverviewRoleAssignmentReadModels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleId",
                table: "ItSystemUsageOverviewRoleAssignmentReadModels",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFullName",
                table: "ItSystemUsageOverviewRoleAssignmentReadModels",
                column: "UserFullName");

            migrationBuilder.CreateIndex(
                name: "IX_UserId",
                table: "ItSystemUsageOverviewRoleAssignmentReadModels",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewSensitiveDataLevelReadModel_Index_SensitiveDataLevel",
                table: "ItSystemUsageOverviewSensitiveDataLevelReadModels",
                column: "SensitivityDataLevel");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOverviewSensitiveDataLevelReadModels_ParentId",
                table: "ItSystemUsageOverviewSensitiveDataLevelReadModels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewTaskRefReadModel_Index_KLEId",
                table: "ItSystemUsageOverviewTaskRefReadModels",
                column: "KLEId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewTaskRefReadModel_Index_KLEName",
                table: "ItSystemUsageOverviewTaskRefReadModels",
                column: "KLEName");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOverviewTaskRefReadModels_ParentId",
                table: "ItSystemUsageOverviewTaskRefReadModels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewUsingSystemUsageReadModel_index_ItSystemUsageId",
                table: "ItSystemUsageOverviewUsingSystemUsageReadModels",
                column: "ItSystemUsageId");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewUsingSystemUsageReadModel_index_ItSystemUsageName",
                table: "ItSystemUsageOverviewUsingSystemUsageReadModels",
                column: "ItSystemUsageName");

            migrationBuilder.CreateIndex(
                name: "ItSystemUsageOverviewUsingSystemUsageReadModel_index_ItSystemUsageUuid",
                table: "ItSystemUsageOverviewUsingSystemUsageReadModels",
                column: "ItSystemUsageUuid");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageOverviewUsingSystemUsageReadModels_ParentId",
                table: "ItSystemUsageOverviewUsingSystemUsageReadModels",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsagePersonalDataOptions_ItSystemUsageId",
                table: "ItSystemUsagePersonalDatas",
                column: "ItSystemUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageSensitiveDataLevels_ItSystemUsage_Id",
                table: "ItSystemUsageSensitiveDataLevels",
                column: "ItSystemUsage_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ItSystemUsageTaskRef_TaskRef_Id",
                table: "TaskRefItSystemUsages",
                column: "TaskRef_Id");

            migrationBuilder.CreateIndex(
                name: "IX_KendoColumnConfiguration_KendoOrganizationalConfigurationId",
                table: "KendoColumnConfigurations",
                column: "KendoOrganizationalConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_KendoOrganizationalConfigurations_LastChangedByUserId",
                table: "KendoOrganizationalConfigurations",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_KendoOrganizationalConfigurations_ObjectOwnerId",
                table: "KendoOrganizationalConfigurations",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_KendoOrganizationalConfigurations_OrganizationId",
                table: "KendoOrganizationalConfigurations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "KendoOrganizationalConfiguration_OverviewType",
                table: "KendoOrganizationalConfigurations",
                column: "OverviewType");

            migrationBuilder.CreateIndex(
                name: "IX_KLEUpdateHistoryItems_LastChangedByUserId",
                table: "KLEUpdateHistoryItems",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_KLEUpdateHistoryItems_ObjectOwnerId",
                table: "KLEUpdateHistoryItems",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_EventType_OccurredAt_EntityType_EventType",
                table: "LifeCycleTrackingEvents",
                columns: new[] { "EventType", "OccurredAtUtc", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_LifeCycleTrackingEvents_EntityUuid",
                table: "LifeCycleTrackingEvents",
                column: "EntityUuid");

            migrationBuilder.CreateIndex(
                name: "IX_LifeCycleTrackingEvents_OptionalOrganizationReferenceId",
                table: "LifeCycleTrackingEvents",
                column: "OptionalOrganizationReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_LifeCycleTrackingEvents_OptionalRightsHolderOrganizationId",
                table: "LifeCycleTrackingEvents",
                column: "OptionalRightsHolderOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LifeCycleTrackingEvents_UserId",
                table: "LifeCycleTrackingEvents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Org_AccessModifier_EventType_OccurredAt_EntityType",
                table: "LifeCycleTrackingEvents",
                columns: new[] { "OptionalOrganizationReferenceId", "OptionalAccessModifier", "EventType", "OccurredAtUtc", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_Org_EventType_OccurredAt_EntityType",
                table: "LifeCycleTrackingEvents",
                columns: new[] { "OptionalOrganizationReferenceId", "EventType", "OccurredAtUtc", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_RightsHolder_EventType_OccurredAt_EntityType",
                table: "LifeCycleTrackingEvents",
                columns: new[] { "OptionalRightsHolderOrganizationId", "EventType", "OccurredAtUtc", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_RightsHolder_Org_EventType_OccurredAt_EntityType",
                table: "LifeCycleTrackingEvents",
                columns: new[] { "OptionalRightsHolderOrganizationId", "OptionalOrganizationReferenceId", "EventType", "OccurredAtUtc", "EntityType" });

            migrationBuilder.CreateIndex(
                name: "IX_LocalAgreementElementTypes_LastChangedByUserId",
                table: "LocalAgreementElementTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalAgreementElementTypes_ObjectOwnerId",
                table: "LocalAgreementElementTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalAgreementElementTypes_OrganizationId",
                table: "LocalAgreementElementTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalArchiveLocations_LastChangedByUserId",
                table: "LocalArchiveLocations",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalArchiveLocations_ObjectOwnerId",
                table: "LocalArchiveLocations",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalArchiveLocations_OrganizationId",
                table: "LocalArchiveLocations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalArchiveTestLocations_LastChangedByUserId",
                table: "LocalArchiveTestLocations",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalArchiveTestLocations_ObjectOwnerId",
                table: "LocalArchiveTestLocations",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalArchiveTestLocations_OrganizationId",
                table: "LocalArchiveTestLocations",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalArchiveTypes_LastChangedByUserId",
                table: "LocalArchiveTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalArchiveTypes_ObjectOwnerId",
                table: "LocalArchiveTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalArchiveTypes_OrganizationId",
                table: "LocalArchiveTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalBusinessTypes_LastChangedByUserId",
                table: "LocalBusinessTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalBusinessTypes_ObjectOwnerId",
                table: "LocalBusinessTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalBusinessTypes_OrganizationId",
                table: "LocalBusinessTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalCriticalityTypes_LastChangedByUserId",
                table: "LocalCriticalityTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalCriticalityTypes_ObjectOwnerId",
                table: "LocalCriticalityTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalCriticalityTypes_OrganizationId",
                table: "LocalCriticalityTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataProcessingBasisForTransferOptions_LastChangedByUserId",
                table: "LocalDataProcessingBasisForTransferOptions",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataProcessingBasisForTransferOptions_ObjectOwnerId",
                table: "LocalDataProcessingBasisForTransferOptions",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataProcessingBasisForTransferOptions_OrganizationId",
                table: "LocalDataProcessingBasisForTransferOptions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataProcessingCountryOptions_LastChangedByUserId",
                table: "LocalDataProcessingCountryOptions",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataProcessingCountryOptions_ObjectOwnerId",
                table: "LocalDataProcessingCountryOptions",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataProcessingCountryOptions_OrganizationId",
                table: "LocalDataProcessingCountryOptions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataProcessingDataResponsibleOptions_LastChangedByUserId",
                table: "LocalDataProcessingDataResponsibleOptions",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataProcessingDataResponsibleOptions_ObjectOwnerId",
                table: "LocalDataProcessingDataResponsibleOptions",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataProcessingDataResponsibleOptions_OrganizationId",
                table: "LocalDataProcessingDataResponsibleOptions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataProcessingOversightOptions_LastChangedByUserId",
                table: "LocalDataProcessingOversightOptions",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataProcessingOversightOptions_ObjectOwnerId",
                table: "LocalDataProcessingOversightOptions",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataProcessingOversightOptions_OrganizationId",
                table: "LocalDataProcessingOversightOptions",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataProcessingRegistrationRoles_LastChangedByUserId",
                table: "LocalDataProcessingRegistrationRoles",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataProcessingRegistrationRoles_ObjectOwnerId",
                table: "LocalDataProcessingRegistrationRoles",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataProcessingRegistrationRoles_OrganizationId",
                table: "LocalDataProcessingRegistrationRoles",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataTypes_LastChangedByUserId",
                table: "LocalDataTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataTypes_ObjectOwnerId",
                table: "LocalDataTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalDataTypes_OrganizationId",
                table: "LocalDataTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalFrequencyTypes_LastChangedByUserId",
                table: "LocalRelationFrequencyTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalFrequencyTypes_ObjectOwnerId",
                table: "LocalRelationFrequencyTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalFrequencyTypes_OrganizationId",
                table: "LocalRelationFrequencyTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalInterfaceTypes_LastChangedByUserId",
                table: "LocalInterfaceTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalInterfaceTypes_ObjectOwnerId",
                table: "LocalInterfaceTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalInterfaceTypes_OrganizationId",
                table: "LocalInterfaceTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalItContractRoles_LastChangedByUserId",
                table: "LocalItContractRoles",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalItContractRoles_ObjectOwnerId",
                table: "LocalItContractRoles",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalItContractRoles_OrganizationId",
                table: "LocalItContractRoles",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalItContractTemplateTypes_LastChangedByUserId",
                table: "LocalItContractTemplateTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalItContractTemplateTypes_ObjectOwnerId",
                table: "LocalItContractTemplateTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalItContractTemplateTypes_OrganizationId",
                table: "LocalItContractTemplateTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalItContractTypes_LastChangedByUserId",
                table: "LocalItContractTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalItContractTypes_ObjectOwnerId",
                table: "LocalItContractTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalItContractTypes_OrganizationId",
                table: "LocalItContractTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalItSystemCategories_LastChangedByUserId",
                table: "LocalItSystemCategories",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalItSystemCategories_ObjectOwnerId",
                table: "LocalItSystemCategories",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalItSystemCategories_OrganizationId",
                table: "LocalItSystemCategories",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalItSystemRoles_LastChangedByUserId",
                table: "LocalItSystemRoles",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalItSystemRoles_ObjectOwnerId",
                table: "LocalItSystemRoles",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalItSystemRoles_OrganizationId",
                table: "LocalItSystemRoles",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalOptionExtendTypes_LastChangedByUserId",
                table: "LocalOptionExtendTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalOptionExtendTypes_ObjectOwnerId",
                table: "LocalOptionExtendTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalOptionExtendTypes_OrganizationId",
                table: "LocalOptionExtendTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalOrganizationUnitRoles_LastChangedByUserId",
                table: "LocalOrganizationUnitRoles",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalOrganizationUnitRoles_ObjectOwnerId",
                table: "LocalOrganizationUnitRoles",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalOrganizationUnitRoles_OrganizationId",
                table: "LocalOrganizationUnitRoles",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalPaymentFreqencyTypes_LastChangedByUserId",
                table: "LocalPaymentFreqencyTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalPaymentFreqencyTypes_ObjectOwnerId",
                table: "LocalPaymentFreqencyTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalPaymentFreqencyTypes_OrganizationId",
                table: "LocalPaymentFreqencyTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalPaymentModelTypes_LastChangedByUserId",
                table: "LocalPaymentModelTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalPaymentModelTypes_ObjectOwnerId",
                table: "LocalPaymentModelTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalPaymentModelTypes_OrganizationId",
                table: "LocalPaymentModelTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalPriceRegulationTypes_LastChangedByUserId",
                table: "LocalPriceRegulationTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalPriceRegulationTypes_ObjectOwnerId",
                table: "LocalPriceRegulationTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalPriceRegulationTypes_OrganizationId",
                table: "LocalPriceRegulationTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalProcurementStrategyTypes_LastChangedByUserId",
                table: "LocalProcurementStrategyTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalProcurementStrategyTypes_ObjectOwnerId",
                table: "LocalProcurementStrategyTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalProcurementStrategyTypes_OrganizationId",
                table: "LocalProcurementStrategyTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalPurchaseFormTypes_LastChangedByUserId",
                table: "LocalPurchaseFormTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalPurchaseFormTypes_ObjectOwnerId",
                table: "LocalPurchaseFormTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalPurchaseFormTypes_OrganizationId",
                table: "LocalPurchaseFormTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalRegisterTypes_LastChangedByUserId",
                table: "LocalRegisterTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalRegisterTypes_ObjectOwnerId",
                table: "LocalRegisterTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalRegisterTypes_OrganizationId",
                table: "LocalRegisterTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalSensitiveDataTypes_LastChangedByUserId",
                table: "LocalSensitiveDataTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalSensitiveDataTypes_ObjectOwnerId",
                table: "LocalSensitiveDataTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalSensitiveDataTypes_OrganizationId",
                table: "LocalSensitiveDataTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalSensitivePersonalDataTypes_LastChangedByUserId",
                table: "LocalSensitivePersonalDataTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalSensitivePersonalDataTypes_ObjectOwnerId",
                table: "LocalSensitivePersonalDataTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalSensitivePersonalDataTypes_OrganizationId",
                table: "LocalSensitivePersonalDataTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalTerminationDeadlineTypes_LastChangedByUserId",
                table: "LocalTerminationDeadlineTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalTerminationDeadlineTypes_ObjectOwnerId",
                table: "LocalTerminationDeadlineTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalTerminationDeadlineTypes_OrganizationId",
                table: "LocalTerminationDeadlineTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionExtendTypes_LastChangedByUserId",
                table: "OptionExtendTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionExtendTypes_ObjectOwnerId",
                table: "OptionExtendTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "OptionExtendTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DEFAULT_ORG",
                table: "Organization",
                column: "IsDefaultOrganization");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_ContactPerson_Id",
                table: "Organization",
                column: "ContactPerson_Id",
                unique: true,
                filter: "[ContactPerson_Id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Cvr",
                table: "Organization",
                column: "Cvr");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_ForeignCountryCodeId",
                table: "Organization",
                column: "ForeignCountryCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_LastChangedByUserId",
                table: "Organization",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_Name",
                table: "Organization",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_ObjectOwnerId",
                table: "Organization",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Organization_TypeId",
                table: "Organization",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "UX_AccessModifier",
                table: "Organization",
                column: "AccessModifier");

            migrationBuilder.CreateIndex(
                name: "UX_Organization_UUID",
                table: "Organization",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationRights_DefaultOrgUnitId",
                table: "OrganizationRights",
                column: "DefaultOrgUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationRights_LastChangedByUserId",
                table: "OrganizationRights",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationRights_ObjectOwnerId",
                table: "OrganizationRights",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationRights_OrganizationId",
                table: "OrganizationRights",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationRights_UserId",
                table: "OrganizationRights",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationSuppliers_OrganizationId",
                table: "OrganizationSuppliers",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_LastChangedByUserId",
                table: "OrganizationUnit",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_ObjectOwnerId",
                table: "OrganizationUnit",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_Origin",
                table: "OrganizationUnit",
                column: "Origin");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_ParentId",
                table: "OrganizationUnit",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_UUID",
                table: "OrganizationUnit",
                column: "ExternalOriginUuid");

            migrationBuilder.CreateIndex(
                name: "UX_LocalId",
                table: "OrganizationUnit",
                columns: new[] { "OrganizationId", "LocalId" },
                unique: true,
                filter: "[LocalId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_OrganizationUnit_UUID",
                table: "OrganizationUnit",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitRights_LastChangedByUserId",
                table: "OrganizationUnitRights",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitRights_ObjectId",
                table: "OrganizationUnitRights",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitRights_ObjectOwnerId",
                table: "OrganizationUnitRights",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitRights_RoleId",
                table: "OrganizationUnitRights",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitRights_UserId",
                table: "OrganizationUnitRights",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitRoles_LastChangedByUserId",
                table: "OrganizationUnitRoles",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitRoles_ObjectOwnerId",
                table: "OrganizationUnitRoles",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "OrganizationUnitRoles",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetRequest_LastChangedByUserId",
                table: "PasswordResetRequest",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetRequest_ObjectOwnerId",
                table: "PasswordResetRequest",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordResetRequest_UserId",
                table: "PasswordResetRequest",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentFreqencyTypes_LastChangedByUserId",
                table: "PaymentFreqencyTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentFreqencyTypes_ObjectOwnerId",
                table: "PaymentFreqencyTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "PaymentFreqencyTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentModelTypes_LastChangedByUserId",
                table: "PaymentModelTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentModelTypes_ObjectOwnerId",
                table: "PaymentModelTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "PaymentModelTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Category",
                table: "PendingReadModelUpdates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_CreatedAt",
                table: "PendingReadModelUpdates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SourceId",
                table: "PendingReadModelUpdates",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceRegulationTypes_LastChangedByUserId",
                table: "PriceRegulationTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceRegulationTypes_ObjectOwnerId",
                table: "PriceRegulationTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "PriceRegulationTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementStrategyTypes_LastChangedByUserId",
                table: "ProcurementStrategyTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcurementStrategyTypes_ObjectOwnerId",
                table: "ProcurementStrategyTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "ProcurementStrategyTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicMessages_LastChangedByUserId",
                table: "PublicMessages",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicMessages_ObjectOwnerId",
                table: "PublicMessages",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_PublicMessage_Uuid",
                table: "PublicMessages",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFormTypes_LastChangedByUserId",
                table: "PurchaseFormTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseFormTypes_ObjectOwnerId",
                table: "PurchaseFormTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "PurchaseFormTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisterTypes_LastChangedByUserId",
                table: "RegisterTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RegisterTypes_ObjectOwnerId",
                table: "RegisterTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_RelationFrequencyTypes_LastChangedByUserId",
                table: "RelationFrequencyTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RelationFrequencyTypes_ObjectOwnerId",
                table: "RelationFrequencyTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "RelationFrequencyTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensitiveDataTypes_LastChangedByUserId",
                table: "SensitiveDataTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SensitiveDataTypes_ObjectOwnerId",
                table: "SensitiveDataTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "SensitiveDataTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensitivePersonalDataTypes_LastChangedByUserId",
                table: "SensitivePersonalDataTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SensitivePersonalDataTypes_ObjectOwnerId",
                table: "SensitivePersonalDataTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SsoOrganizationIdentities_Organization_Id",
                table: "StsOrganizationIdentities",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "UX_ExternalUuid",
                table: "StsOrganizationIdentities",
                column: "ExternalUuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SsoUserIdentities_User_Id",
                table: "SsoUserIdentities",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "UX_ExternalUuid",
                table: "SsoUserIdentities",
                column: "ExternalUuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLogName",
                table: "StsOrganizationChangeLogs",
                column: "ResponsibleUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChangeLogResponsibleType",
                table: "StsOrganizationChangeLogs",
                column: "ResponsibleType");

            migrationBuilder.CreateIndex(
                name: "IX_LogTime",
                table: "StsOrganizationChangeLogs",
                column: "LogTime");

            migrationBuilder.CreateIndex(
                name: "IX_StsOrganizationChangeLogs_LastChangedByUserId",
                table: "StsOrganizationChangeLogs",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StsOrganizationChangeLogs_ObjectOwnerId",
                table: "StsOrganizationChangeLogs",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_StsOrganizationChangeLogs_StsOrganizationConnectionId",
                table: "StsOrganizationChangeLogs",
                column: "StsOrganizationConnectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Connected",
                table: "StsOrganizationConnections",
                column: "Connected");

            migrationBuilder.CreateIndex(
                name: "IX_DateOfLatestCheckBySubscription",
                table: "StsOrganizationConnections",
                column: "DateOfLatestCheckBySubscription");

            migrationBuilder.CreateIndex(
                name: "IX_Required",
                table: "StsOrganizationConnections",
                column: "SubscribeToUpdates");

            migrationBuilder.CreateIndex(
                name: "IX_StsOrganizationConnections_LastChangedByUserId",
                table: "StsOrganizationConnections",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StsOrganizationConnections_ObjectOwnerId",
                table: "StsOrganizationConnections",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_StsOrganizationConnections_OrganizationId",
                table: "StsOrganizationConnections",
                column: "OrganizationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StsOrganizationConsequenceLogs_ChangeLogId",
                table: "StsOrganizationConsequenceLogs",
                column: "ChangeLogId");

            migrationBuilder.CreateIndex(
                name: "IX_StsOrganizationConsequenceLogs_LastChangedByUserId",
                table: "StsOrganizationConsequenceLogs",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_StsOrganizationConsequenceLogs_ObjectOwnerId",
                table: "StsOrganizationConsequenceLogs",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_StsOrganizationConsequenceType",
                table: "StsOrganizationConsequenceLogs",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_StsOrganizationConsequenceUuid",
                table: "StsOrganizationConsequenceLogs",
                column: "ExternalUnitUuid");

            migrationBuilder.CreateIndex(
                name: "IX_SubDataProcessors_DataProcessingRegistrationId",
                table: "SubDataProcessors",
                column: "DataProcessingRegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_SubDataProcessors_InsecureCountryId",
                table: "SubDataProcessors",
                column: "InsecureCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_SubDataProcessors_SubDataProcessorBasisForTransferId",
                table: "SubDataProcessors",
                column: "SubDataProcessorBasisForTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemRelations_AssociatedContractId",
                table: "SystemRelations",
                column: "AssociatedContractId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemRelations_FromSystemUsageId",
                table: "SystemRelations",
                column: "FromSystemUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemRelations_LastChangedByUserId",
                table: "SystemRelations",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemRelations_ObjectOwnerId",
                table: "SystemRelations",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemRelations_RelationInterfaceId",
                table: "SystemRelations",
                column: "RelationInterfaceId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemRelations_ToSystemUsageId",
                table: "SystemRelations",
                column: "ToSystemUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemRelations_UsageFrequencyId",
                table: "SystemRelations",
                column: "UsageFrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskRef_LastChangedByUserId",
                table: "TaskRef",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskRef_ObjectOwnerId",
                table: "TaskRef",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskRef_OwnedByOrganizationUnitId",
                table: "TaskRef",
                column: "OwnedByOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskRef_ParentId",
                table: "TaskRef",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "UX_TaskKey",
                table: "TaskRef",
                column: "TaskKey",
                unique: true,
                filter: "[TaskKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_TaskRef_Uuid",
                table: "TaskRef",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskRefItSystemUsageOptOut_TaskRef_Id",
                table: "TaskRefItSystemUsageOptOut",
                column: "TaskRef_Id");

            migrationBuilder.CreateIndex(
                name: "IX_TerminationDeadlineTypes_LastChangedByUserId",
                table: "TerminationDeadlineTypes",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminationDeadlineTypes_ObjectOwnerId",
                table: "TerminationDeadlineTypes",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_Option_Uuid",
                table: "TerminationDeadlineTypes",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Text_LastChangedByUserId",
                table: "Text",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Text_ObjectOwnerId",
                table: "Text",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_UIModuleCustomizations_LastChangedByUserId",
                table: "UIModuleCustomizations",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UIModuleCustomizations_ObjectOwnerId",
                table: "UIModuleCustomizations",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "UX_OrganizationId_UIModuleCustomization_Module",
                table: "UIModuleCustomizations",
                columns: new[] { "OrganizationId", "Module" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_IsSystemIntegrator",
                table: "User",
                column: "IsSystemIntegrator");

            migrationBuilder.CreateIndex(
                name: "IX_User_LastChangedByUserId",
                table: "User",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_ObjectOwnerId",
                table: "User",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "User_Index_Email",
                table: "User",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "User_Index_Name",
                table: "User",
                columns: new[] { "Name", "LastName" });

            migrationBuilder.CreateIndex(
                name: "UX_User_Uuid",
                table: "User",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_DataProcessingRegistration_Id",
                table: "UserNotifications",
                column: "DataProcessingRegistration_Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_Itcontract_Id",
                table: "UserNotifications",
                column: "Itcontract_Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_ItSystemUsage_Id",
                table: "UserNotifications",
                column: "ItSystemUsage_Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_LastChangedByUserId",
                table: "UserNotifications",
                column: "LastChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_NotificationRecipientId",
                table: "UserNotifications",
                column: "NotificationRecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_ObjectOwnerId",
                table: "UserNotifications",
                column: "ObjectOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_OrganizationId",
                table: "UserNotifications",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArchivePeriod_ItSystemUsage_ItSystemUsageId",
                table: "ArchivePeriod",
                column: "ItSystemUsageId",
                principalTable: "ItSystemUsage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BrokenLinkInExternalReference_ExternalReferences_BrokenReferenceOrigin_Id",
                table: "BrokenLinkInExternalReferences",
                column: "BrokenReferenceOrigin_Id",
                principalTable: "ExternalReferences",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DataProcessingCountryOptionDataProcessingRegistration_DataProcessingRegistrations_DataProcessingRegistration_Id",
                table: "DataProcessingRegistrationDataProcessingCountryOptions",
                column: "DataProcessingRegistration_Id",
                principalTable: "DataProcessingRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataProcessingOversightOptionDataProcessingRegistration_DataProcessingRegistrations_DataProcessingRegistration_Id",
                table: "DataProcessingRegistrationDataProcessingOversightOptions",
                column: "DataProcessingRegistration_Id",
                principalTable: "DataProcessingRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataProcessingRegistrationItContract_DataProcessingRegistrations_DataProcessingRegistration_Id",
                table: "ItContractDataProcessingRegistrations",
                column: "DataProcessingRegistration_Id",
                principalTable: "DataProcessingRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataProcessingRegistrationItContract_ItContract_ItContract_Id",
                table: "ItContractDataProcessingRegistrations",
                column: "ItContract_Id",
                principalTable: "ItContract",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataProcessingRegistrationItSystemUsage_DataProcessingRegistrations_DataProcessingRegistration_Id",
                table: "DataProcessingRegistrationItSystemUsages",
                column: "DataProcessingRegistration_Id",
                principalTable: "DataProcessingRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataProcessingRegistrationItSystemUsage_ItSystemUsage_ItSystemUsage_Id",
                table: "DataProcessingRegistrationItSystemUsages",
                column: "ItSystemUsage_Id",
                principalTable: "ItSystemUsage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataProcessingRegistrationOrganization_DataProcessingRegistrations_DataProcessingRegistration_Id",
                table: "DataProcessingRegistrationOrganizations",
                column: "DataProcessingRegistration_Id",
                principalTable: "DataProcessingRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataProcessingRegistrationOversightDates_DataProcessingRegistrations_ParentId",
                table: "DataProcessingRegistrationOversightDates",
                column: "ParentId",
                principalTable: "DataProcessingRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataProcessingRegistrationReadModels_DataProcessingRegistrations_SourceEntityId",
                table: "DataProcessingRegistrationReadModels",
                column: "SourceEntityId",
                principalTable: "DataProcessingRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DataProcessingRegistrationRights_DataProcessingRegistrations_ObjectId",
                table: "DataProcessingRegistrationRights",
                column: "ObjectId",
                principalTable: "DataProcessingRegistrations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataProcessingRegistrations_ExternalReferences_ReferenceId",
                table: "DataProcessingRegistrations",
                column: "ReferenceId",
                principalTable: "ExternalReferences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataProcessingRegistrations_ItContract_MainContractId",
                table: "DataProcessingRegistrations",
                column: "MainContractId",
                principalTable: "ItContract",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EconomyStream_ItContract_ExternPaymentForId",
                table: "EconomyStream",
                column: "ExternPaymentForId",
                principalTable: "ItContract",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EconomyStream_ItContract_InternPaymentForId",
                table: "EconomyStream",
                column: "InternPaymentForId",
                principalTable: "ItContract",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exhibit_ItSystem_ItSystemId",
                table: "Exhibit",
                column: "ItSystemId",
                principalTable: "ItSystem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalReferences_ItContract_ItContract_Id",
                table: "ExternalReferences",
                column: "ItContract_Id",
                principalTable: "ItContract",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalReferences_ItSystemUsage_ItSystemUsage_Id",
                table: "ExternalReferences",
                column: "ItSystemUsage_Id",
                principalTable: "ItSystemUsage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExternalReferences_ItSystem_ItSystem_Id",
                table: "ExternalReferences",
                column: "ItSystem_Id",
                principalTable: "ItSystem",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArchiveLocation_User_LastChangedByUserId",
                table: "ArchiveLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_ArchiveLocation_User_ObjectOwnerId",
                table: "ArchiveLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_ArchiveTestLocation_User_LastChangedByUserId",
                table: "ArchiveTestLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_ArchiveTestLocation_User_ObjectOwnerId",
                table: "ArchiveTestLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_ArchiveTypes_User_LastChangedByUserId",
                table: "ArchiveTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ArchiveTypes_User_ObjectOwnerId",
                table: "ArchiveTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessTypes_User_LastChangedByUserId",
                table: "BusinessTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_BusinessTypes_User_ObjectOwnerId",
                table: "BusinessTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactPersons_User_LastChangedByUserId",
                table: "ContactPersons");

            migrationBuilder.DropForeignKey(
                name: "FK_ContactPersons_User_ObjectOwnerId",
                table: "ContactPersons");

            migrationBuilder.DropForeignKey(
                name: "FK_CountryCodes_User_LastChangedByUserId",
                table: "CountryCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_CountryCodes_User_ObjectOwnerId",
                table: "CountryCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_CriticalityTypes_User_LastChangedByUserId",
                table: "CriticalityTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_CriticalityTypes_User_ObjectOwnerId",
                table: "CriticalityTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_DataProcessingBasisForTransferOptions_User_LastChangedByUserId",
                table: "DataProcessingBasisForTransferOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_DataProcessingBasisForTransferOptions_User_ObjectOwnerId",
                table: "DataProcessingBasisForTransferOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_DataProcessingDataResponsibleOptions_User_LastChangedByUserId",
                table: "DataProcessingDataResponsibleOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_DataProcessingDataResponsibleOptions_User_ObjectOwnerId",
                table: "DataProcessingDataResponsibleOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_DataProcessingRegistrations_User_LastChangedByUserId",
                table: "DataProcessingRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_DataProcessingRegistrations_User_ObjectOwnerId",
                table: "DataProcessingRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalReferences_User_LastChangedByUserId",
                table: "ExternalReferences");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalReferences_User_ObjectOwnerId",
                table: "ExternalReferences");

            migrationBuilder.DropForeignKey(
                name: "FK_ItContract_User_LastChangedByUserId",
                table: "ItContract");

            migrationBuilder.DropForeignKey(
                name: "FK_ItContract_User_ObjectOwnerId",
                table: "ItContract");

            migrationBuilder.DropForeignKey(
                name: "FK_ItContractTemplateTypes_User_LastChangedByUserId",
                table: "ItContractTemplateTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ItContractTemplateTypes_User_ObjectOwnerId",
                table: "ItContractTemplateTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ItContractTypes_User_LastChangedByUserId",
                table: "ItContractTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ItContractTypes_User_ObjectOwnerId",
                table: "ItContractTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ItSystem_User_LastChangedByUserId",
                table: "ItSystem");

            migrationBuilder.DropForeignKey(
                name: "FK_ItSystem_User_ObjectOwnerId",
                table: "ItSystem");

            migrationBuilder.DropForeignKey(
                name: "FK_ItSystemCategories_User_LastChangedByUserId",
                table: "ItSystemCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ItSystemCategories_User_ObjectOwnerId",
                table: "ItSystemCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ItSystemUsage_User_LastChangedByUserId",
                table: "ItSystemUsage");

            migrationBuilder.DropForeignKey(
                name: "FK_ItSystemUsage_User_ObjectOwnerId",
                table: "ItSystemUsage");

            migrationBuilder.DropForeignKey(
                name: "FK_OptionExtendTypes_User_LastChangedByUserId",
                table: "OptionExtendTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_OptionExtendTypes_User_ObjectOwnerId",
                table: "OptionExtendTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Organization_User_LastChangedByUserId",
                table: "Organization");

            migrationBuilder.DropForeignKey(
                name: "FK_Organization_User_ObjectOwnerId",
                table: "Organization");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUnit_User_LastChangedByUserId",
                table: "OrganizationUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationUnit_User_ObjectOwnerId",
                table: "OrganizationUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentFreqencyTypes_User_LastChangedByUserId",
                table: "PaymentFreqencyTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentFreqencyTypes_User_ObjectOwnerId",
                table: "PaymentFreqencyTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentModelTypes_User_LastChangedByUserId",
                table: "PaymentModelTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentModelTypes_User_ObjectOwnerId",
                table: "PaymentModelTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceRegulationTypes_User_LastChangedByUserId",
                table: "PriceRegulationTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_PriceRegulationTypes_User_ObjectOwnerId",
                table: "PriceRegulationTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcurementStrategyTypes_User_LastChangedByUserId",
                table: "ProcurementStrategyTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ProcurementStrategyTypes_User_ObjectOwnerId",
                table: "ProcurementStrategyTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFormTypes_User_LastChangedByUserId",
                table: "PurchaseFormTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseFormTypes_User_ObjectOwnerId",
                table: "PurchaseFormTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisterTypes_User_LastChangedByUserId",
                table: "RegisterTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_RegisterTypes_User_ObjectOwnerId",
                table: "RegisterTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_SensitiveDataTypes_User_LastChangedByUserId",
                table: "SensitiveDataTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_SensitiveDataTypes_User_ObjectOwnerId",
                table: "SensitiveDataTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_SensitivePersonalDataTypes_User_LastChangedByUserId",
                table: "SensitivePersonalDataTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_SensitivePersonalDataTypes_User_ObjectOwnerId",
                table: "SensitivePersonalDataTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_TerminationDeadlineTypes_User_LastChangedByUserId",
                table: "TerminationDeadlineTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_TerminationDeadlineTypes_User_ObjectOwnerId",
                table: "TerminationDeadlineTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_ExternalReferences_ItSystemUsage_ItSystemUsage_Id",
                table: "ExternalReferences");

            migrationBuilder.DropForeignKey(
                name: "FK_DataProcessingRegistrations_ExternalReferences_ReferenceId",
                table: "DataProcessingRegistrations");

            migrationBuilder.DropForeignKey(
                name: "FK_ItContract_ExternalReferences_ReferenceId",
                table: "ItContract");

            migrationBuilder.DropForeignKey(
                name: "FK_ItSystem_ExternalReferences_ReferenceId",
                table: "ItSystem");

            migrationBuilder.DropTable(
                name: "AdviceSents");

            migrationBuilder.DropTable(
                name: "AdviceUserRelations");

            migrationBuilder.DropTable(
                name: "ArchivePeriod");

            migrationBuilder.DropTable(
                name: "AttachedOptions");

            migrationBuilder.DropTable(
                name: "BrokenLinkInExternalReferences");

            migrationBuilder.DropTable(
                name: "BrokenLinkInInterfaces");

            migrationBuilder.DropTable(
                name: "Config");

            migrationBuilder.DropTable(
                name: "CustomizedUiNodes");

            migrationBuilder.DropTable(
                name: "DataProcessingRegistrationDataProcessingCountryOptions");

            migrationBuilder.DropTable(
                name: "DataProcessingRegistrationDataProcessingOversightOptions");

            migrationBuilder.DropTable(
                name: "ItContractDataProcessingRegistrations");

            migrationBuilder.DropTable(
                name: "DataProcessingRegistrationItSystemUsages");

            migrationBuilder.DropTable(
                name: "DataProcessingRegistrationOrganizations");

            migrationBuilder.DropTable(
                name: "DataProcessingRegistrationOversightDates");

            migrationBuilder.DropTable(
                name: "DataProcessingRegistrationRights");

            migrationBuilder.DropTable(
                name: "DataProcessingRegistrationRoleAssignmentReadModels");

            migrationBuilder.DropTable(
                name: "DataProtectionAdvisors");

            migrationBuilder.DropTable(
                name: "DataResponsibles");

            migrationBuilder.DropTable(
                name: "DataRow");

            migrationBuilder.DropTable(
                name: "EconomyStream");

            migrationBuilder.DropTable(
                name: "Exhibit");

            migrationBuilder.DropTable(
                name: "HelpTexts");

            migrationBuilder.DropTable(
                name: "ItContractAgreementElementTypes");

            migrationBuilder.DropTable(
                name: "ItContractItSystemUsages");

            migrationBuilder.DropTable(
                name: "ItContractOverviewReadModelDataProcessingAgreements");

            migrationBuilder.DropTable(
                name: "ItContractOverviewReadModelItSystemUsages");

            migrationBuilder.DropTable(
                name: "ItContractOverviewReadModelSystemRelations");

            migrationBuilder.DropTable(
                name: "ItContractOverviewRoleAssignmentReadModels");

            migrationBuilder.DropTable(
                name: "ItContractRights");

            migrationBuilder.DropTable(
                name: "ItSystemRights");

            migrationBuilder.DropTable(
                name: "TaskRefItSystems");

            migrationBuilder.DropTable(
                name: "ItSystemUsageOrgUnitUsages");

            migrationBuilder.DropTable(
                name: "ItSystemUsageOverviewArchivePeriodReadModels");

            migrationBuilder.DropTable(
                name: "ItSystemUsageOverviewDataProcessingRegistrationReadModels");

            migrationBuilder.DropTable(
                name: "ItSystemUsageOverviewInterfaceReadModels");

            migrationBuilder.DropTable(
                name: "ItSystemUsageOverviewItContractReadModels");

            migrationBuilder.DropTable(
                name: "ItSystemUsageOverviewUsedBySystemUsageReadModels");

            migrationBuilder.DropTable(
                name: "ItSystemUsageOverviewRelevantOrgUnitReadModels");

            migrationBuilder.DropTable(
                name: "ItSystemUsageOverviewRoleAssignmentReadModels");

            migrationBuilder.DropTable(
                name: "ItSystemUsageOverviewSensitiveDataLevelReadModels");

            migrationBuilder.DropTable(
                name: "ItSystemUsageOverviewTaskRefReadModels");

            migrationBuilder.DropTable(
                name: "ItSystemUsageOverviewUsingSystemUsageReadModels");

            migrationBuilder.DropTable(
                name: "ItSystemUsagePersonalDatas");

            migrationBuilder.DropTable(
                name: "ItSystemUsageSensitiveDataLevels");

            migrationBuilder.DropTable(
                name: "TaskRefItSystemUsages");

            migrationBuilder.DropTable(
                name: "KendoColumnConfigurations");

            migrationBuilder.DropTable(
                name: "KLEUpdateHistoryItems");

            migrationBuilder.DropTable(
                name: "LifeCycleTrackingEvents");

            migrationBuilder.DropTable(
                name: "LocalAgreementElementTypes");

            migrationBuilder.DropTable(
                name: "LocalArchiveLocations");

            migrationBuilder.DropTable(
                name: "LocalArchiveTestLocations");

            migrationBuilder.DropTable(
                name: "LocalArchiveTypes");

            migrationBuilder.DropTable(
                name: "LocalBusinessTypes");

            migrationBuilder.DropTable(
                name: "LocalCriticalityTypes");

            migrationBuilder.DropTable(
                name: "LocalDataProcessingBasisForTransferOptions");

            migrationBuilder.DropTable(
                name: "LocalDataProcessingCountryOptions");

            migrationBuilder.DropTable(
                name: "LocalDataProcessingDataResponsibleOptions");

            migrationBuilder.DropTable(
                name: "LocalDataProcessingOversightOptions");

            migrationBuilder.DropTable(
                name: "LocalDataProcessingRegistrationRoles");

            migrationBuilder.DropTable(
                name: "LocalDataTypes");

            migrationBuilder.DropTable(
                name: "LocalRelationFrequencyTypes");

            migrationBuilder.DropTable(
                name: "LocalInterfaceTypes");

            migrationBuilder.DropTable(
                name: "LocalItContractRoles");

            migrationBuilder.DropTable(
                name: "LocalItContractTemplateTypes");

            migrationBuilder.DropTable(
                name: "LocalItContractTypes");

            migrationBuilder.DropTable(
                name: "LocalItSystemCategories");

            migrationBuilder.DropTable(
                name: "LocalItSystemRoles");

            migrationBuilder.DropTable(
                name: "LocalOptionExtendTypes");

            migrationBuilder.DropTable(
                name: "LocalOrganizationUnitRoles");

            migrationBuilder.DropTable(
                name: "LocalPaymentFreqencyTypes");

            migrationBuilder.DropTable(
                name: "LocalPaymentModelTypes");

            migrationBuilder.DropTable(
                name: "LocalPriceRegulationTypes");

            migrationBuilder.DropTable(
                name: "LocalProcurementStrategyTypes");

            migrationBuilder.DropTable(
                name: "LocalPurchaseFormTypes");

            migrationBuilder.DropTable(
                name: "LocalRegisterTypes");

            migrationBuilder.DropTable(
                name: "LocalSensitiveDataTypes");

            migrationBuilder.DropTable(
                name: "LocalSensitivePersonalDataTypes");

            migrationBuilder.DropTable(
                name: "LocalTerminationDeadlineTypes");

            migrationBuilder.DropTable(
                name: "OrganizationRights");

            migrationBuilder.DropTable(
                name: "OrganizationSuppliers");

            migrationBuilder.DropTable(
                name: "OrganizationUnitRights");

            migrationBuilder.DropTable(
                name: "PasswordResetRequest");

            migrationBuilder.DropTable(
                name: "PendingReadModelUpdates");

            migrationBuilder.DropTable(
                name: "PublicMessages");

            migrationBuilder.DropTable(
                name: "StsOrganizationIdentities");

            migrationBuilder.DropTable(
                name: "SsoUserIdentities");

            migrationBuilder.DropTable(
                name: "StsOrganizationConsequenceLogs");

            migrationBuilder.DropTable(
                name: "SubDataProcessors");

            migrationBuilder.DropTable(
                name: "SystemRelations");

            migrationBuilder.DropTable(
                name: "TaskRefItSystemUsageOptOut");

            migrationBuilder.DropTable(
                name: "Text");

            migrationBuilder.DropTable(
                name: "UserNotifications");

            migrationBuilder.DropTable(
                name: "Advice");

            migrationBuilder.DropTable(
                name: "BrokenExternalReferencesReports");

            migrationBuilder.DropTable(
                name: "UIModuleCustomizations");

            migrationBuilder.DropTable(
                name: "DataProcessingOversightOptions");

            migrationBuilder.DropTable(
                name: "DataProcessingRegistrationRoles");

            migrationBuilder.DropTable(
                name: "DataProcessingRegistrationReadModels");

            migrationBuilder.DropTable(
                name: "DataTypes");

            migrationBuilder.DropTable(
                name: "AgreementElementTypes");

            migrationBuilder.DropTable(
                name: "ItContractOverviewReadModels");

            migrationBuilder.DropTable(
                name: "ItContractRoles");

            migrationBuilder.DropTable(
                name: "ItSystemRoles");

            migrationBuilder.DropTable(
                name: "ItSystemUsageOverviewReadModels");

            migrationBuilder.DropTable(
                name: "KendoOrganizationalConfigurations");

            migrationBuilder.DropTable(
                name: "OrganizationUnitRoles");

            migrationBuilder.DropTable(
                name: "StsOrganizationChangeLogs");

            migrationBuilder.DropTable(
                name: "DataProcessingCountryOptions");

            migrationBuilder.DropTable(
                name: "ItInterface");

            migrationBuilder.DropTable(
                name: "RelationFrequencyTypes");

            migrationBuilder.DropTable(
                name: "TaskRef");

            migrationBuilder.DropTable(
                name: "StsOrganizationConnections");

            migrationBuilder.DropTable(
                name: "InterfaceTypes");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "ItSystemUsage");

            migrationBuilder.DropTable(
                name: "ArchiveLocations");

            migrationBuilder.DropTable(
                name: "ArchiveTestLocations");

            migrationBuilder.DropTable(
                name: "ArchiveTypes");

            migrationBuilder.DropTable(
                name: "ItSystemCategories");

            migrationBuilder.DropTable(
                name: "RegisterTypes");

            migrationBuilder.DropTable(
                name: "SensitiveDataTypes");

            migrationBuilder.DropTable(
                name: "ExternalReferences");

            migrationBuilder.DropTable(
                name: "DataProcessingRegistrations");

            migrationBuilder.DropTable(
                name: "ItSystem");

            migrationBuilder.DropTable(
                name: "DataProcessingBasisForTransferOptions");

            migrationBuilder.DropTable(
                name: "DataProcessingDataResponsibleOptions");

            migrationBuilder.DropTable(
                name: "ItContract");

            migrationBuilder.DropTable(
                name: "BusinessTypes");

            migrationBuilder.DropTable(
                name: "SensitivePersonalDataTypes");

            migrationBuilder.DropTable(
                name: "CriticalityTypes");

            migrationBuilder.DropTable(
                name: "ItContractTemplateTypes");

            migrationBuilder.DropTable(
                name: "ItContractTypes");

            migrationBuilder.DropTable(
                name: "OptionExtendTypes");

            migrationBuilder.DropTable(
                name: "OrganizationUnit");

            migrationBuilder.DropTable(
                name: "PaymentFreqencyTypes");

            migrationBuilder.DropTable(
                name: "PaymentModelTypes");

            migrationBuilder.DropTable(
                name: "PriceRegulationTypes");

            migrationBuilder.DropTable(
                name: "ProcurementStrategyTypes");

            migrationBuilder.DropTable(
                name: "PurchaseFormTypes");

            migrationBuilder.DropTable(
                name: "TerminationDeadlineTypes");

            migrationBuilder.DropTable(
                name: "Organization");

            migrationBuilder.DropTable(
                name: "ContactPersons");

            migrationBuilder.DropTable(
                name: "CountryCodes");

            migrationBuilder.DropTable(
                name: "OrganizationTypes");
        }
    }
}
