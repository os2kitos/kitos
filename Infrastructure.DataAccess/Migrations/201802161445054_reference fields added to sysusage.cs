namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class referencefieldsaddedtosysusage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "datahandlerSupervisionDocumentationUrl", c => c.String());
            AddColumn("dbo.ItSystemUsage", "TechnicalSupervisionDocumentationUrl", c => c.String());
            AddColumn("dbo.ItSystemUsage", "UserSupervisionDocumentationUrl", c => c.String());
            AddColumn("dbo.ItSystemUsage", "RiskSupervisionDocumentationUrl", c => c.String());
            AddColumn("dbo.ItSystemUsage", "DPIASupervisionDocumentationUrl", c => c.String());
            AddColumn("dbo.ItSystemUsage", "DataHearingSupervisionDocumentationUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsage", "DataHearingSupervisionDocumentationUrl");
            DropColumn("dbo.ItSystemUsage", "DPIASupervisionDocumentationUrl");
            DropColumn("dbo.ItSystemUsage", "RiskSupervisionDocumentationUrl");
            DropColumn("dbo.ItSystemUsage", "UserSupervisionDocumentationUrl");
            DropColumn("dbo.ItSystemUsage", "TechnicalSupervisionDocumentationUrl");
            DropColumn("dbo.ItSystemUsage", "datahandlerSupervisionDocumentationUrl");
        }
    }
}
