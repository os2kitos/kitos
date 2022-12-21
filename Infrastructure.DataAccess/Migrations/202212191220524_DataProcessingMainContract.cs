namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataProcessingMainContract : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrations", "MainContractId", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataProcessingRegistrationReadModels", "ActiveAccordingToMainContract", c => c.Boolean(nullable: false));
            CreateIndex("dbo.DataProcessingRegistrations", "MainContractId");
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "IsActive", name: "IX_DPR_IsActive");
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "ActiveAccordingToMainContract", name: "IX_DPR_MainContractIsActive");
            AddForeignKey("dbo.DataProcessingRegistrations", "MainContractId", "dbo.ItContract", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProcessingRegistrations", "MainContractId", "dbo.ItContract");
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DPR_MainContractIsActive");
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DPR_IsActive");
            DropIndex("dbo.DataProcessingRegistrations", new[] { "MainContractId" });
            DropColumn("dbo.DataProcessingRegistrationReadModels", "ActiveAccordingToMainContract");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "IsActive");
            DropColumn("dbo.DataProcessingRegistrations", "MainContractId");
        }
    }
}
