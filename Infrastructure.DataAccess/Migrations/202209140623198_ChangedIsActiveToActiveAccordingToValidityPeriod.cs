namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedIsActiveToActiveAccordingToValidityPeriod : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "ActiveAccordingToValidityPeriod", c => c.Boolean(nullable: false));
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "IsActive");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "IsActive", c => c.Boolean(nullable: false));
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "ActiveAccordingToValidityPeriod");
        }
    }
}
