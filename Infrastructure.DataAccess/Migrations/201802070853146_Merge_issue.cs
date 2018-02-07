namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Merge_issue : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "noteUsage", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsage", "noteUsage");
        }
    }
}
