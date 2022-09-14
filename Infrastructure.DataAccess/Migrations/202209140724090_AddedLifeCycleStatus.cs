namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedLifeCycleStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "LifeCycleStatus", c => c.Int());
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "LifeCycleStatus", c => c.Int());
            CreateIndex("dbo.ItSystemUsage", "LifeCycleStatus", name: "ItSystemUsage_Index_LifeCycleStatus");
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
            Sql(@"UPDATE dbo.ItSystemUsage
                  SET LifeCycleStatus = 
                        CASE Active
							WHEN 0 THEN 0
							WHEN 1 THEN 3
						END;"
            );
            DropIndex("dbo.ItSystemUsage", "ItSystemUsage_Index_LifeCycleStatus");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "LifeCycleStatus");
            DropColumn("dbo.ItSystemUsage", "LifeCycleStatus");
        }
    }
}
