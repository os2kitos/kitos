namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Usage_ai_technology_field_as_yesNoUndecided : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ItSystemUsage", "ContainsAITechnology", c => c.Int());
            AlterColumn("dbo.ItSystemUsageOverviewReadModels", "ContainsAITechnology", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ItSystemUsageOverviewReadModels", "ContainsAITechnology", c => c.Boolean());
            AlterColumn("dbo.ItSystemUsage", "ContainsAITechnology", c => c.Boolean());
        }
    }
}
