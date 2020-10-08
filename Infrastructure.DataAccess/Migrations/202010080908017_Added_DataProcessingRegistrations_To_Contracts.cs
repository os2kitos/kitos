namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_DataProcessingRegistrations_To_Contracts : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItContractDataProcessingRegistrations",
                c => new
                    {
                        ItContract_Id = c.Int(nullable: false),
                        DataProcessingRegistration_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItContract_Id, t.DataProcessingRegistration_Id })
                .ForeignKey("dbo.ItContract", t => t.ItContract_Id)
                .ForeignKey("dbo.DataProcessingRegistrations", t => t.DataProcessingRegistration_Id)
                .Index(t => t.ItContract_Id)
                .Index(t => t.DataProcessingRegistration_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItContractDataProcessingRegistrations", "DataProcessingRegistration_Id", "dbo.DataProcessingRegistrations");
            DropForeignKey("dbo.ItContractDataProcessingRegistrations", "ItContract_Id", "dbo.ItContract");
            DropIndex("dbo.ItContractDataProcessingRegistrations", new[] { "DataProcessingRegistration_Id" });
            DropIndex("dbo.ItContractDataProcessingRegistrations", new[] { "ItContract_Id" });
            DropTable("dbo.ItContractDataProcessingRegistrations");
        }
    }
}
