namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddWebAccessbilityFieldsToItSystemUsage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "WebAccessibilityCompliance", c => c.Int());
            AddColumn("dbo.ItSystemUsage", "LastWebAccessibilityCheck", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystemUsage", "WebAccessibilityNotes", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsage", "WebAccessibilityNotes");
            DropColumn("dbo.ItSystemUsage", "LastWebAccessibilityCheck");
            DropColumn("dbo.ItSystemUsage", "WebAccessibilityCompliance");
        }
    }
}
