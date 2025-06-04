namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddWebAccessibilityFieldsToUsageReadModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "WebAccessibilityCompliance", c => c.Int());
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "LastWebAccessibilityCheck", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "WebAccessibilityNotes", c => c.String());
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "WebAccessibilityCompliance");
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "LastWebAccessibilityCheck");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItSystemUsageOverviewReadModels", new[] { "LastWebAccessibilityCheck" });
            DropIndex("dbo.ItSystemUsageOverviewReadModels", new[] { "WebAccessibilityCompliance" });
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "WebAccessibilityNotes");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "LastWebAccessibilityCheck");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "WebAccessibilityCompliance");
        }
    }
}
