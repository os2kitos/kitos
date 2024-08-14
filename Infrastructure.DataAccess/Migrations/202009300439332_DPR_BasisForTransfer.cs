namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DPR_BasisForTransfer : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.DataProcessingRegistrations", name: "DataProcessingBasisForTransferOption_Id", newName: "BasisForTransferId");
            RenameIndex(table: "dbo.DataProcessingRegistrations", name: "IX_DataProcessingBasisForTransferOption_Id", newName: "IX_BasisForTransferId");
            AddColumn("dbo.DataProcessingRegistrationReadModels", "BasisForTransferName", c => c.String(maxLength: 100));
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "BasisForTransferName", name: "IX_DRP_BasisForTransfer");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DRP_BasisForTransfer");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "BasisForTransferName");
            RenameIndex(table: "dbo.DataProcessingRegistrations", name: "IX_BasisForTransferId", newName: "IX_DataProcessingBasisForTransferOption_Id");
            RenameColumn(table: "dbo.DataProcessingRegistrations", name: "BasisForTransferId", newName: "DataProcessingBasisForTransferOption_Id");
        }
    }
}
