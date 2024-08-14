namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dataprocessingreadmodeluuids : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DRP_BasisForTransfer");
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DPR_DataResponsible");
            AddColumn("dbo.DataProcessingRegistrationReadModels", "BasisForTransferName", c => c.String(maxLength: 100));
            AddColumn("dbo.DataProcessingRegistrationReadModels", "BasisForTransferUuid", c => c.Guid());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsibleName", c => c.String(maxLength: 100));
            AddColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsibleUuid", c => c.Guid());
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "BasisForTransferName", name: "IX_DRP_BasisForTransfer");
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "BasisForTransferUuid", name: "IX_DRP_BasisForTransferUuid");
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "DataResponsibleName", name: "IX_DPR_DataResponsible");
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "DataResponsibleUuid", name: "IX_DPR_DataResponsibleUuid");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "BasisForTransfer");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsible");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsible", c => c.String(maxLength: 100));
            AddColumn("dbo.DataProcessingRegistrationReadModels", "BasisForTransfer", c => c.String(maxLength: 100));
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DPR_DataResponsibleUuid");
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DPR_DataResponsible");
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DRP_BasisForTransferUuid");
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DRP_BasisForTransfer");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsibleUuid");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "DataResponsibleName");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "BasisForTransferUuid");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "BasisForTransferName");
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "DataResponsible", name: "IX_DPR_DataResponsible");
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "BasisForTransfer", name: "IX_DRP_BasisForTransfer");
        }
    }
}
