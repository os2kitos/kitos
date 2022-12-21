namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Changed_Dpr_SubDataProcessors : DbMigration
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
                        Id = c.Int(nullable: false, identity: true),
                        OrganizationId = c.Int(nullable: false),
                        SubDataProcessorBasisForTransferId = c.Int(),
                        TransferToInsecureCountry = c.Int(),
                        InsecureCountryId = c.Int(),
                        DataProcessingRegistrationId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DataProcessingCountryOptions", t => t.InsecureCountryId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .ForeignKey("dbo.DataProcessingBasisForTransferOptions", t => t.SubDataProcessorBasisForTransferId)
                .ForeignKey("dbo.DataProcessingRegistrations", t => t.DataProcessingRegistrationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.SubDataProcessorBasisForTransferId)
                .Index(t => t.InsecureCountryId)
                .Index(t => t.DataProcessingRegistrationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
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
            
            DropForeignKey("dbo.SubDataProcessors", "DataProcessingRegistrationId", "dbo.DataProcessingRegistrations");
            DropForeignKey("dbo.SubDataProcessors", "SubDataProcessorBasisForTransferId", "dbo.DataProcessingBasisForTransferOptions");
            DropForeignKey("dbo.SubDataProcessors", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.SubDataProcessors", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.SubDataProcessors", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.SubDataProcessors", "InsecureCountryId", "dbo.DataProcessingCountryOptions");
            DropIndex("dbo.SubDataProcessors", new[] { "LastChangedByUserId" });
            DropIndex("dbo.SubDataProcessors", new[] { "ObjectOwnerId" });
            DropIndex("dbo.SubDataProcessors", new[] { "DataProcessingRegistrationId" });
            DropIndex("dbo.SubDataProcessors", new[] { "InsecureCountryId" });
            DropIndex("dbo.SubDataProcessors", new[] { "SubDataProcessorBasisForTransferId" });
            DropIndex("dbo.SubDataProcessors", new[] { "OrganizationId" });
            DropTable("dbo.SubDataProcessors");
            CreateIndex("dbo.DataProcessingRegistrationOrganization1", "Organization_Id");
            CreateIndex("dbo.DataProcessingRegistrationOrganization1", "DataProcessingRegistration_Id");
            AddForeignKey("dbo.DataProcessingRegistrationOrganization1", "Organization_Id", "dbo.Organization", "Id");
            AddForeignKey("dbo.DataProcessingRegistrationOrganization1", "DataProcessingRegistration_Id", "dbo.DataProcessingRegistrations", "Id");
        }
    }
}
