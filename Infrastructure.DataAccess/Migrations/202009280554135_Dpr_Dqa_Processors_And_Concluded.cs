namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Dpr_Dqa_Processors_And_Concluded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataProcessingRegistrationOrganizations",
                c => new
                    {
                        DataProcessingRegistration_Id = c.Int(nullable: false),
                        Organization_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.DataProcessingRegistration_Id, t.Organization_Id })
                .ForeignKey("dbo.DataProcessingRegistrations", t => t.DataProcessingRegistration_Id)
                .ForeignKey("dbo.Organization", t => t.Organization_Id)
                .Index(t => t.DataProcessingRegistration_Id)
                .Index(t => t.Organization_Id);
            
            CreateTable(
                "dbo.DataProcessingRegistrationOrganization1",
                c => new
                    {
                        DataProcessingRegistration_Id = c.Int(nullable: false),
                        Organization_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.DataProcessingRegistration_Id, t.Organization_Id })
                .ForeignKey("dbo.DataProcessingRegistrations", t => t.DataProcessingRegistration_Id)
                .ForeignKey("dbo.Organization", t => t.Organization_Id)
                .Index(t => t.DataProcessingRegistration_Id)
                .Index(t => t.Organization_Id);
            
            AddColumn("dbo.DataProcessingRegistrations", "HasSubDataProcessors", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrations", "IsAgreementConcluded", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrations", "AgreementConcludedAt", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.DataProcessingRegistrationReadModels", "DataProcessorNamesAsCsv", c => c.String());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "SubDataProcessorNamesAsCsv", c => c.String());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "IsAgreementConcluded", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "AgreementConcludedAt", c => c.DateTime(precision: 7, storeType: "datetime2"));
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "IsAgreementConcluded", name: "IX_DPR_Concluded");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProcessingRegistrationOrganization1", "Organization_Id", "dbo.Organization");
            DropForeignKey("dbo.DataProcessingRegistrationOrganization1", "DataProcessingRegistration_Id", "dbo.DataProcessingRegistrations");
            DropForeignKey("dbo.DataProcessingRegistrationOrganizations", "Organization_Id", "dbo.Organization");
            DropForeignKey("dbo.DataProcessingRegistrationOrganizations", "DataProcessingRegistration_Id", "dbo.DataProcessingRegistrations");
            DropIndex("dbo.DataProcessingRegistrationOrganization1", new[] { "Organization_Id" });
            DropIndex("dbo.DataProcessingRegistrationOrganization1", new[] { "DataProcessingRegistration_Id" });
            DropIndex("dbo.DataProcessingRegistrationOrganizations", new[] { "Organization_Id" });
            DropIndex("dbo.DataProcessingRegistrationOrganizations", new[] { "DataProcessingRegistration_Id" });
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DPR_Concluded");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "AgreementConcludedAt");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "IsAgreementConcluded");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "SubDataProcessorNamesAsCsv");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "DataProcessorNamesAsCsv");
            DropColumn("dbo.DataProcessingRegistrations", "AgreementConcludedAt");
            DropColumn("dbo.DataProcessingRegistrations", "IsAgreementConcluded");
            DropColumn("dbo.DataProcessingRegistrations", "HasSubDataProcessors");
            DropTable("dbo.DataProcessingRegistrationOrganization1");
            DropTable("dbo.DataProcessingRegistrationOrganizations");
        }
    }
}
