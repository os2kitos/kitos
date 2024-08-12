namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ItContractReferenceUuids : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContractOverviewReadModels", "ParentContractUuid", c => c.Guid());
            AddColumn("dbo.ItContractOverviewReadModelDataProcessingAgreements", "DataProcessingRegistrationUuid", c => c.Guid(nullable: false));
            CreateIndex("dbo.ItContractOverviewReadModels", "ParentContractUuid", name: "IX_ParentContract_Uuid");
            CreateIndex("dbo.ItContractOverviewReadModelDataProcessingAgreements", "DataProcessingRegistrationUuid", name: "IX_ItContract_Read_Dpr_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItContractOverviewReadModelDataProcessingAgreements", "IX_ItContract_Read_Dpr_Uuid");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_ParentContract_Uuid");
            DropColumn("dbo.ItContractOverviewReadModelDataProcessingAgreements", "DataProcessingRegistrationUuid");
            DropColumn("dbo.ItContractOverviewReadModels", "ParentContractUuid");
        }
    }
}
