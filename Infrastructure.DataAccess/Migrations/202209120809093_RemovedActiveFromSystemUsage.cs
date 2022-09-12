namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedActiveFromSystemUsage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "LifeCycleStatus", c => c.Int());
            CreateIndex("dbo.ItSystemUsage", "LifeCycleStatus", name: "ItSystemUsage_Index_LifeCycleStatus");
            DropColumn("dbo.ItSystemUsage", "Active");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsage", "Active", c => c.Boolean(nullable: false));
            DropIndex("dbo.ItSystemUsage", "ItSystemUsage_Index_LifeCycleStatus");
            DropColumn("dbo.ItSystemUsage", "LifeCycleStatus");
        }
    }
}
