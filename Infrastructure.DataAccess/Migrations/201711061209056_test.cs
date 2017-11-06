namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ItContractAgreementElementTypes", newName: "ItContractAgreementElementTypes1");
            CreateTable(
                "dbo.ItContractAgreementElementTypes",
                c => new
                    {
                        ItContract_Id = c.Int(nullable: false),
                        AgreementElementType_Id = c.Int(nullable: false),
                        AgreementElementType_Id1 = c.Int(),
                        ItContract_Id1 = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItContract_Id, t.AgreementElementType_Id })
                .ForeignKey("dbo.AgreementElementTypes", t => t.AgreementElementType_Id1)
                .ForeignKey("dbo.ItContract", t => t.ItContract_Id1)
                .Index(t => t.AgreementElementType_Id1)
                .Index(t => t.ItContract_Id1);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItContractAgreementElementTypes", "ItContract_Id1", "dbo.ItContract");
            DropForeignKey("dbo.ItContractAgreementElementTypes", "AgreementElementType_Id1", "dbo.AgreementElementTypes");
            DropIndex("dbo.ItContractAgreementElementTypes", new[] { "ItContract_Id1" });
            DropIndex("dbo.ItContractAgreementElementTypes", new[] { "AgreementElementType_Id1" });
            DropTable("dbo.ItContractAgreementElementTypes");
            RenameTable(name: "dbo.ItContractAgreementElementTypes1", newName: "ItContractAgreementElementTypes");
        }
    }
}
