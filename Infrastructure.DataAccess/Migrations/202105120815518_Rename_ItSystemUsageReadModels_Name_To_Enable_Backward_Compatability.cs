namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Rename_ItSystemUsageReadModels_Name_To_Enable_Backward_Compatability : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_Name");
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "SystemName", c => c.String(nullable: false, maxLength: 100));
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "SystemName", name: "ItSystemUsageOverviewReadModel_Index_Name");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "Name");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "Name", c => c.String(nullable: false, maxLength: 100));
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_Name");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "SystemName");
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "Name", name: "ItSystemUsageOverviewReadModel_Index_Name");
        }
    }
}
