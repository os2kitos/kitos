namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Nameforurlinsysusageaddedandconvictiondataremovved : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "datahandlerSupervisionDocumentationUrlName", c => c.String());
            AddColumn("dbo.ItSystemUsage", "TechnicalSupervisionDocumentationUrlName", c => c.String());
            AddColumn("dbo.ItSystemUsage", "UserSupervisionDocumentationUrlName", c => c.String());
            AddColumn("dbo.ItSystemUsage", "RiskSupervisionDocumentationUrlName", c => c.String());
            AddColumn("dbo.ItSystemUsage", "DPIASupervisionDocumentationUrlName", c => c.String());
            AddColumn("dbo.ItSystemUsage", "DataHearingSupervisionDocumentationUrlName", c => c.String());
            DropColumn("dbo.ItSystemUsage", "convictionsData");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsage", "convictionsData", c => c.Int(nullable: false));
            DropColumn("dbo.ItSystemUsage", "DataHearingSupervisionDocumentationUrlName");
            DropColumn("dbo.ItSystemUsage", "DPIASupervisionDocumentationUrlName");
            DropColumn("dbo.ItSystemUsage", "RiskSupervisionDocumentationUrlName");
            DropColumn("dbo.ItSystemUsage", "UserSupervisionDocumentationUrlName");
            DropColumn("dbo.ItSystemUsage", "TechnicalSupervisionDocumentationUrlName");
            DropColumn("dbo.ItSystemUsage", "datahandlerSupervisionDocumentationUrlName");
        }
    }
}
