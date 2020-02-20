using System.Linq;

namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Migrate_InterfaceUsage_To_SystemRelations : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItInterfaceUsage", "MigratedToUuid", c => c.Guid(nullable: true));
            SqlResource(GetType().Assembly.GetManifestResourceNames().First(x => x.Contains("Migrate_Contract_Relations.sql")));
        }

        public override void Down()
        {
            DropColumn("dbo.ItInterfaceUsage", "MigratedToUuid");
        }
    }
}
