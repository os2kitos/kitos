namespace Infrastructure.DataAccess.Migrations
{
    using Infrastructure.DataAccess.Tools;
    using System;
    using System.Data.Entity.Migrations;

    public partial class change_dpr_sub_data_processors : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DataProcessingRegistrationOrganization1", "DataProcessingRegistration_Id", "dbo.DataProcessingRegistrations");
            DropForeignKey("dbo.DataProcessingRegistrationOrganization1", "Organization_Id", "dbo.Organization");
            DropIndex("dbo.DataProcessingRegistrationOrganization1", new[] { "DataProcessingRegistration_Id" });
            DropIndex("dbo.DataProcessingRegistrationOrganization1", new[] { "Organization_Id" });
            CreateTable(
                "dbo.SubDataProcessors",
                c => new
                {
                    OrganizationId = c.Int(nullable: false),
                    DataProcessingRegistrationId = c.Int(nullable: false),
                    SubDataProcessorBasisForTransferId = c.Int(),
                    TransferToInsecureCountry = c.Int(),
                    InsecureCountryId = c.Int(),
                })
                .PrimaryKey(t => new { t.OrganizationId, t.DataProcessingRegistrationId })
                .ForeignKey("dbo.DataProcessingRegistrations", t => t.DataProcessingRegistrationId, cascadeDelete: true)
                .ForeignKey("dbo.DataProcessingCountryOptions", t => t.InsecureCountryId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .ForeignKey("dbo.DataProcessingBasisForTransferOptions", t => t.SubDataProcessorBasisForTransferId)
                .Index(t => t.OrganizationId)
                .Index(t => t.DataProcessingRegistrationId)
                .Index(t => t.SubDataProcessorBasisForTransferId)
                .Index(t => t.InsecureCountryId);

            //Migrate old sub data processor registrations into the new structure
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Dpr_Migrate_SubDataProcessors.sql"));
            DropTable("dbo.DataProcessingRegistrationOrganization1");
        }

        public override void Down()
        {
            CreateTable(
                "dbo.DataProcessingRegistrationOrganization1",
                c => new
                {
                    DataProcessingRegistration_Id = c.Int(nullable: false),
                    Organization_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.DataProcessingRegistration_Id, t.Organization_Id });

            DropForeignKey("dbo.SubDataProcessors", "SubDataProcessorBasisForTransferId", "dbo.DataProcessingBasisForTransferOptions");
            DropForeignKey("dbo.SubDataProcessors", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.SubDataProcessors", "InsecureCountryId", "dbo.DataProcessingCountryOptions");
            DropForeignKey("dbo.SubDataProcessors", "DataProcessingRegistrationId", "dbo.DataProcessingRegistrations");
            DropIndex("dbo.SubDataProcessors", new[] { "InsecureCountryId" });
            DropIndex("dbo.SubDataProcessors", new[] { "SubDataProcessorBasisForTransferId" });
            DropIndex("dbo.SubDataProcessors", new[] { "DataProcessingRegistrationId" });
            DropIndex("dbo.SubDataProcessors", new[] { "OrganizationId" });
            DropTable("dbo.SubDataProcessors");
            CreateIndex("dbo.DataProcessingRegistrationOrganization1", "Organization_Id");
            CreateIndex("dbo.DataProcessingRegistrationOrganization1", "DataProcessingRegistration_Id");
            AddForeignKey("dbo.DataProcessingRegistrationOrganization1", "Organization_Id", "dbo.Organization", "Id");
            AddForeignKey("dbo.DataProcessingRegistrationOrganization1", "DataProcessingRegistration_Id", "dbo.DataProcessingRegistrations", "Id");
        }
    }
}
