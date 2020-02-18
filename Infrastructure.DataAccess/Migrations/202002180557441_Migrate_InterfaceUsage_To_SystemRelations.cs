namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Migrate_InterfaceUsage_To_SystemRelations : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItInterfaceUsage", "MigratedToUuid", c => c.Guid(nullable:true));
            //TODO: Call SQL with the data migration script
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItInterfaceUsage", "MigratedToUuid");
        }
    }
}
