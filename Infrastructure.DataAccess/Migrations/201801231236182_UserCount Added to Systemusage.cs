namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserCountAddedtoSystemusage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "UserCount", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsage", "UserCount");
        }
    }
}
