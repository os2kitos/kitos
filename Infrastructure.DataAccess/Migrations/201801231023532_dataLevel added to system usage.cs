namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dataLeveladdedtosystemusage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "DataLevel", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsage", "DataLevel");
        }
    }
}
