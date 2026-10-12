using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.DataAccess.Migrations.EfCore
{
    /// <inheritdoc />
    public partial class BridgeMissingColumnsFromEF6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // These columns exist in the EF Core model but were never added by EF6 migrations on
            // existing databases. All other structural differences (shadow FK column names such as
            // ItSystemUsage_Id, TaskRef_Id, etc.) use EF6's naming convention, which EF Core inherits
            // as shadow property names — so existing EF6 databases already have the correct column names
            // and no renaming is required here.
            // Idempotent guards are used because some EF6 databases may already contain these objects.

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'dbo' AND table_name = 'ItSystem' AND column_name = 'SensitivePersonalDataTypeId') THEN
                        ALTER TABLE dbo.""ItSystem"" ADD COLUMN ""SensitivePersonalDataTypeId"" int NULL;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_ItSystem_SensitivePersonalDataTypes_SensitivePersonalDataTypeId' AND table_schema = 'dbo') THEN
                        ALTER TABLE dbo.""ItSystem"" ADD CONSTRAINT ""FK_ItSystem_SensitivePersonalDataTypes_SensitivePersonalDataTypeId""
                            FOREIGN KEY (""SensitivePersonalDataTypeId"") REFERENCES dbo.""SensitivePersonalDataTypes"" (""Id"");
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_ItSystem_SensitivePersonalDataTypeId"" ON dbo.""ItSystem"" (""SensitivePersonalDataTypeId"");
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'dbo' AND table_name = 'ItSystemUsage' AND column_name = 'RegisterTypeId') THEN
                        ALTER TABLE dbo.""ItSystemUsage"" ADD COLUMN ""RegisterTypeId"" int NULL;
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM information_schema.table_constraints WHERE constraint_name = 'FK_ItSystemUsage_RegisterTypes_RegisterTypeId' AND table_schema = 'dbo') THEN
                        ALTER TABLE dbo.""ItSystemUsage"" ADD CONSTRAINT ""FK_ItSystemUsage_RegisterTypes_RegisterTypeId""
                            FOREIGN KEY (""RegisterTypeId"") REFERENCES dbo.""RegisterTypes"" (""Id"");
                    END IF;
                END $$;
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_ItSystemUsage_RegisterTypeId"" ON dbo.""ItSystemUsage"" (""RegisterTypeId"");
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItSystemUsage_RegisterTypeId",
                table: "ItSystemUsage");

            migrationBuilder.DropForeignKey(
                name: "FK_ItSystemUsage_RegisterTypes_RegisterTypeId",
                table: "ItSystemUsage");

            migrationBuilder.DropColumn(
                name: "RegisterTypeId",
                table: "ItSystemUsage");

            migrationBuilder.DropIndex(
                name: "IX_ItSystem_SensitivePersonalDataTypeId",
                table: "ItSystem");

            migrationBuilder.DropForeignKey(
                name: "FK_ItSystem_SensitivePersonalDataTypes_SensitivePersonalDataTypeId",
                table: "ItSystem");

            migrationBuilder.DropColumn(
                name: "SensitivePersonalDataTypeId",
                table: "ItSystem");
        }
    }
}