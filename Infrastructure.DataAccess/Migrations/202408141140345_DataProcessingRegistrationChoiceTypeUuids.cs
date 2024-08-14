namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataProcessingRegistrationChoiceTypeUuids : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrationReadModels", "BasisForTransferUuid", c => c.Guid());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsibleUuid", c => c.Guid());
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "BasisForTransferUuid", name: "IX_DRP_BasisForTransferUuid");
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "DataResponsibleUuid", name: "IX_DPR_DataResponsibleUuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DPR_DataResponsibleUuid");
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DRP_BasisForTransferUuid");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsibleUuid");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "BasisForTransferUuid");
        }
    }
}
