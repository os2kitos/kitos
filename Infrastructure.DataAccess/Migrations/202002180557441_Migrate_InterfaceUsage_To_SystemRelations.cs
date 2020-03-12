using Infrastructure.DataAccess.Tools;

namespace Infrastructure.DataAccess.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Migrate_InterfaceUsage_To_SystemRelations : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItInterfaceUsage", "MigratedToUuid", c => c.Guid(nullable: true));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Migrate_Contract_Relations.sql"));
        }

        public override void Down()
        {
            DropColumn("dbo.ItInterfaceUsage", "MigratedToUuid");
        }
    }
}
