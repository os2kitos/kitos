namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class isBusinessCriticaladdedtosystemusage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "isBusinessCritical", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsage", "isBusinessCritical");
        }
    }
}
