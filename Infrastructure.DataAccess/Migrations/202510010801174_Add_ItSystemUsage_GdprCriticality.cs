namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_ItSystemUsage_GdprCriticality : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "GdprCriticality", c => c.Int());
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "GdprCriticality", c => c.Int());
            CreateIndex("dbo.ItSystemUsage", "GdprCriticality", name: "ItSystemUsage_Index_GdprCriticality");
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "GdprCriticality", name: "ItSystemUsageOverviewReadModel_Index_GdprCriticality");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_GdprCriticality");
            DropIndex("dbo.ItSystemUsage", "ItSystemUsage_Index_GdprCriticality");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "GdprCriticality");
            DropColumn("dbo.ItSystemUsage", "GdprCriticality");
        }
    }
}
