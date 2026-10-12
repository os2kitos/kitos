namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_DPR_OversightReportLinks : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrationReadModels", "LatestOversightReportLink", c => c.String());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "LatestOversightReportLinkName", c => c.String());
            AddColumn("dbo.DataProcessingRegistrationOversightDates", "OversightReportLink", c => c.String());
            AddColumn("dbo.DataProcessingRegistrationOversightDates", "OversightReportLinkName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataProcessingRegistrationOversightDates", "OversightReportLinkName");
            DropColumn("dbo.DataProcessingRegistrationOversightDates", "OversightReportLink");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "LatestOversightReportLinkName");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "LatestOversightReportLink");
        }
    }
}
