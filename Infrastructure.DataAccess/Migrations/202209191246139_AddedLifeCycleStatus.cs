namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedLifeCycleStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "LifeCycleStatus", c => c.Int());
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "ActiveAccordingToLifeCycle", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "LifeCycleStatus", c => c.Int());
            CreateIndex("dbo.ItSystemUsage", "LifeCycleStatus", name: "ItSystemUsage_Index_LifeCycleStatus");
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "ActiveAccordingToValidityPeriod", name: "ItSystemUsageOverviewReadModel_Index_ActiveAccordingToValidityPeriod");
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "ActiveAccordingToLifeCycle", name: "ItSystemUsageOverviewReadModel_Index_ActiveAccordingToLifeCycle");
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "LifeCycleStatus", name: "ItSystemUsageOverviewReadModel_Index_LifeCycleStatus");
            Sql(@"UPDATE dbo.ItSystemUsage
                  SET LifeCycleStatus = 
                        CASE Active
							WHEN 0 THEN 0
							WHEN 1 THEN 3
						END;"
            );
            DropColumn("dbo.ItSystemUsage", "Active");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsage", "Active", c => c.Boolean(nullable: false));
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_LifeCycleStatus");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ActiveAccordingToLifeCycle");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ActiveAccordingToValidityPeriod");
            DropIndex("dbo.ItSystemUsage", "ItSystemUsage_Index_LifeCycleStatus");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "LifeCycleStatus");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "ActiveAccordingToLifeCycle");
            DropColumn("dbo.ItSystemUsage", "LifeCycleStatus");
        }
    }
}
