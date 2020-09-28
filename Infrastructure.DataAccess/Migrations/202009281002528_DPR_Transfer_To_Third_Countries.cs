namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DPR_Transfer_To_Third_Countries : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataProcessingRegistrationDataProcessingCountryOptions",
                c => new
                    {
                        DataProcessingRegistration_Id = c.Int(nullable: false),
                        DataProcessingCountryOption_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.DataProcessingRegistration_Id, t.DataProcessingCountryOption_Id })
                .ForeignKey("dbo.DataProcessingRegistrations", t => t.DataProcessingRegistration_Id)
                .ForeignKey("dbo.DataProcessingCountryOptions", t => t.DataProcessingCountryOption_Id)
                .Index(t => t.DataProcessingRegistration_Id)
                .Index(t => t.DataProcessingCountryOption_Id);
            
            AddColumn("dbo.DataProcessingRegistrations", "TransferToInsecureThirdCountries", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "TransferToInsecureThirdCountries", c => c.Int());
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "TransferToInsecureThirdCountries", name: "IX_DPR_TransferToInsecureThirdCountries");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProcessingRegistrationDataProcessingCountryOptions", "DataProcessingCountryOption_Id", "dbo.DataProcessingCountryOptions");
            DropForeignKey("dbo.DataProcessingRegistrationDataProcessingCountryOptions", "DataProcessingRegistration_Id", "dbo.DataProcessingRegistrations");
            DropIndex("dbo.DataProcessingRegistrationDataProcessingCountryOptions", new[] { "DataProcessingCountryOption_Id" });
            DropIndex("dbo.DataProcessingRegistrationDataProcessingCountryOptions", new[] { "DataProcessingRegistration_Id" });
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DPR_TransferToInsecureThirdCountries");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "TransferToInsecureThirdCountries");
            DropColumn("dbo.DataProcessingRegistrations", "TransferToInsecureThirdCountries");
            DropTable("dbo.DataProcessingRegistrationDataProcessingCountryOptions");
        }
    }
}
