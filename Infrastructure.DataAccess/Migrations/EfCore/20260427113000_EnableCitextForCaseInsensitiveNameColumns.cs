using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    [DbContext(typeof(KitosContext))]
    [Migration("20260427113000_EnableCitextForCaseInsensitiveNameColumns")]
    public partial class EnableCitextForCaseInsensitiveNameColumns : Migration
    {
        private static readonly string[] OptionTables =
        {
            "AgreementElementTypes",
            "ArchiveTypes",
            "BusinessTypes",
            "CriticalityTypes",
            "DataProcessingRegistrationRoles",
            "DataTypes",
            "InterfaceTypes",
            "ItContractRoles",
            "ItContractTemplateTypes",
            "ItContractTypes",
            "ItSystemCategories",
            "ItSystemRoles",
            "OptionExtendTypes",
            "OrganizationUnitRoles",
            "PaymentFreqencyTypes",
            "PaymentModelTypes",
            "PriceRegulationTypes",
            "ProcurementStrategyTypes",
            "PurchaseFormTypes",
            "RegisterTypes",
            "RelationFrequencyTypes",
            "SensitiveDataTypes",
            "SensitivePersonalDataTypes",
            "TerminationDeadlineTypes"
        };

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // citext must be installed in the public schema so it is accessible via PostgreSQL's
            // default search_path ("$user",public). The public schema is recreated here if it was
            // dropped, keeping app tables in dbo as intended.
            migrationBuilder.Sql("CREATE SCHEMA IF NOT EXISTS public; CREATE EXTENSION IF NOT EXISTS citext SCHEMA public;");

            AlterColumnToCitext(migrationBuilder, "ItSystem", "Name");
            AlterColumnToCitext(migrationBuilder, "ItInterface", "Name");
            AlterColumnToCitext(migrationBuilder, "ItContract", "Name");
            AlterColumnToCitext(migrationBuilder, "Organization", "Name");
            AlterColumnToCitext(migrationBuilder, "OrganizationUnit", "Name");
            AlterColumnToCitext(migrationBuilder, "DataProcessingRegistrations", "Name");

            foreach (var table in OptionTables)
            {
                AlterColumnToCitext(migrationBuilder, table, "Name");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            AlterColumnToType(migrationBuilder, "ItSystem", "Name", "character varying(100)");
            AlterColumnToType(migrationBuilder, "ItInterface", "Name", "character varying(100)");
            AlterColumnToType(migrationBuilder, "ItContract", "Name", "character varying(200)");
            AlterColumnToType(migrationBuilder, "Organization", "Name", "text");
            AlterColumnToType(migrationBuilder, "OrganizationUnit", "Name", "text");
            AlterColumnToType(migrationBuilder, "DataProcessingRegistrations", "Name", "character varying(200)");

            foreach (var table in OptionTables)
            {
                AlterColumnToType(migrationBuilder, table, "Name", "character varying(150)");
            }
        }

        private static void AlterColumnToCitext(MigrationBuilder migrationBuilder, string tableName, string columnName)
        {
            AlterColumnToType(migrationBuilder, tableName, columnName, "citext");
        }

        private static void AlterColumnToType(MigrationBuilder migrationBuilder, string tableName, string columnName, string typeName)
        {
            // Target the canonical 'dbo' schema only
            migrationBuilder.Sql($"ALTER TABLE dbo.\"{tableName}\" ALTER COLUMN \"{columnName}\" TYPE {typeName};");
        }
    }
}
