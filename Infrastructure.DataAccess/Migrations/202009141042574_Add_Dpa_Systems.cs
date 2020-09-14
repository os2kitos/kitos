namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Dpa_Systems : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataProcessingAgreementItSystemUsages",
                c => new
                    {
                        DataProcessingAgreement_Id = c.Int(nullable: false),
                        ItSystemUsage_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.DataProcessingAgreement_Id, t.ItSystemUsage_Id })
                .ForeignKey("dbo.DataProcessingAgreements", t => t.DataProcessingAgreement_Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .Index(t => t.DataProcessingAgreement_Id)
                .Index(t => t.ItSystemUsage_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProcessingAgreementItSystemUsages", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.DataProcessingAgreementItSystemUsages", "DataProcessingAgreement_Id", "dbo.DataProcessingAgreements");
            DropIndex("dbo.DataProcessingAgreementItSystemUsages", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.DataProcessingAgreementItSystemUsages", new[] { "DataProcessingAgreement_Id" });
            DropTable("dbo.DataProcessingAgreementItSystemUsages");
        }
    }
}
