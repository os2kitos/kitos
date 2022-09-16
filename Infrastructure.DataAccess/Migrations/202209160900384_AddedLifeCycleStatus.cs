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
            DropColumn("dbo.ItSystemUsage", "Active");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsage", "Active", c => c.Boolean(nullable: false));
            DropIndex("dbo.ItSystemUsage", "ItSystemUsage_Index_LifeCycleStatus");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "LifeCycleStatus");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "ActiveAccordingToLifeCycle");
            DropColumn("dbo.ItSystemUsage", "LifeCycleStatus");
        }
    }
}
