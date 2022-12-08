namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OversightScheduledInspectionDateField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrations", "OversightScheduledInspectionDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.DataProcessingRegistrationReadModels", "OversightScheduledInspectionDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "OversightScheduledInspectionDate", name: "IX_DPR_OversightScheduledInspectionDate");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DPR_OversightScheduledInspectionDate");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "OversightScheduledInspectionDate");
            DropColumn("dbo.DataProcessingRegistrations", "OversightScheduledInspectionDate");
        }
    }
}
