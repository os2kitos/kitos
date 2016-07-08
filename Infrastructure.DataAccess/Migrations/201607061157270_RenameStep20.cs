namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep20 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.AgreementElements", newName: "AgreementElementTypes");
            RenameTable(name: "dbo.ItContractAgreementElements", newName: "ItContractAgreementElementTypes");
            RenameColumn(table: "dbo.ItContractAgreementElementTypes", name: "ItContractId", newName: "ItContract_Id");
            RenameColumn(table: "dbo.ItContractAgreementElementTypes", name: "ElemId", newName: "AgreementElementType_Id");
            RenameIndex(table: "dbo.ItContractAgreementElementTypes", name: "IX_ItContractId", newName: "IX_ItContract_Id");
            RenameIndex(table: "dbo.ItContractAgreementElementTypes", name: "IX_ElemId", newName: "IX_AgreementElementType_Id");
        }

        public override void Down()
        {
            RenameIndex(table: "dbo.ItContractAgreementElementTypes", name: "IX_AgreementElementType_Id", newName: "IX_ElemId");
            RenameIndex(table: "dbo.ItContractAgreementElementTypes", name: "IX_ItContract_Id", newName: "IX_ItContractId");
            RenameColumn(table: "dbo.ItContractAgreementElementTypes", name: "AgreementElementType_Id", newName: "ElemId");
            RenameColumn(table: "dbo.ItContractAgreementElementTypes", name: "ItContract_Id", newName: "ItContractId");
            RenameTable(name: "dbo.ItContractAgreementElementTypes", newName: "ItContractAgreementElements");
        }
    }
}
