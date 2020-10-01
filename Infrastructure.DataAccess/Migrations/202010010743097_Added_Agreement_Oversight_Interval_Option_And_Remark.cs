namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Agreement_Oversight_Interval_Option_And_Remark : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrations", "OversightInterval", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrations", "OversightIntervalRemark", c => c.String());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "OversightInterval", c => c.Int());
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "OversightInterval", name: "IX_DPR_OversightInterval");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DPR_OversightInterval");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "OversightInterval");
            DropColumn("dbo.DataProcessingRegistrations", "OversightIntervalRemark");
            DropColumn("dbo.DataProcessingRegistrations", "OversightInterval");
        }
    }
}
