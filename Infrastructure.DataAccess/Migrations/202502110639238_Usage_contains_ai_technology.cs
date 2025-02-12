namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Usage_contains_ai_technology : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "ContainsAITechnology", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsage", "ContainsAITechnology");
        }
    }
}
