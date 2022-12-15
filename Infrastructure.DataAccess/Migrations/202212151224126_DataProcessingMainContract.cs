namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataProcessingMainContract : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrations", "MainContractId", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "MainContractId", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "MainContractIsActive", c => c.Boolean(nullable: false));
            CreateIndex("dbo.DataProcessingRegistrations", "MainContractId");
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "MainContractId", name: "IX_DPR_MainContractId");
            AddForeignKey("dbo.DataProcessingRegistrations", "MainContractId", "dbo.ItContract", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProcessingRegistrations", "MainContractId", "dbo.ItContract");
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DPR_MainContractId");
            DropIndex("dbo.DataProcessingRegistrations", new[] { "MainContractId" });
            DropColumn("dbo.DataProcessingRegistrationReadModels", "MainContractIsActive");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "MainContractId");
            DropColumn("dbo.DataProcessingRegistrations", "MainContractId");
        }
    }
}
