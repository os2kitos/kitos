namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SystemUsageOverViewReadModelSystemActive : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "SystemActive", c => c.Boolean(nullable: false));
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "SystemActive", name: "ItSystemUsageOverviewReadModel_Index_SystemActive");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_SystemActive");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "SystemActive");
        }
    }
}
