namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ContractDataProcessingAgreementUuid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContractOverviewReadModelDataProcessingAgreements", "DataProcessingRegistrationUuid", c => c.Guid(nullable: false));
            CreateIndex("dbo.ItContractOverviewReadModelDataProcessingAgreements", "DataProcessingRegistrationUuid", name: "IX_ItContract_Read_Dpr_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItContractOverviewReadModelDataProcessingAgreements", "IX_ItContract_Read_Dpr_Uuid");
            DropColumn("dbo.ItContractOverviewReadModelDataProcessingAgreements", "DataProcessingRegistrationUuid");
        }
    }
}
