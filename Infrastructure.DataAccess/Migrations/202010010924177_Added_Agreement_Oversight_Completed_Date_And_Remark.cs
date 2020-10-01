namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Agreement_Oversight_Completed_Date_And_Remark : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrations", "IsOversightCompleted", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrations", "LatestOversightDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.DataProcessingRegistrations", "IsOversightCompletedRemark", c => c.String());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "IsOversightCompleted", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "LatestOversightDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "IsOversightCompleted", name: "IX_DPR_IsOversightCompleted");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DPR_IsOversightCompleted");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "LatestOversightDate");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "IsOversightCompleted");
            DropColumn("dbo.DataProcessingRegistrations", "IsOversightCompletedRemark");
            DropColumn("dbo.DataProcessingRegistrations", "LatestOversightDate");
            DropColumn("dbo.DataProcessingRegistrations", "IsOversightCompleted");
        }
    }
}
