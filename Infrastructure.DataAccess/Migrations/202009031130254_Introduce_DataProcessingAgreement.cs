namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Introduce_DataProcessingAgreement : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataProcessingAgreementReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        OrganizationId = c.Int(nullable: false),
                        SourceEntityId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .ForeignKey("dbo.DataProcessingAgreements", t => t.SourceEntityId)
                .Index(t => t.Name, name: "DataProcessingAgreementReadModel_Index_Name")
                .Index(t => t.OrganizationId)
                .Index(t => t.SourceEntityId);
            
            CreateTable(
                "dbo.DataProcessingAgreements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        OrganizationId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .Index(t => t.Name, name: "DataProcessingAgreement_Index_Name")
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.PendingReadModelUpdates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SourceId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Category = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProcessingAgreementReadModels", "SourceEntityId", "dbo.DataProcessingAgreements");
            DropForeignKey("dbo.DataProcessingAgreements", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.DataProcessingAgreements", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProcessingAgreements", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataProcessingAgreementReadModels", "OrganizationId", "dbo.Organization");
            DropIndex("dbo.DataProcessingAgreements", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataProcessingAgreements", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataProcessingAgreements", new[] { "OrganizationId" });
            DropIndex("dbo.DataProcessingAgreements", "DataProcessingAgreement_Index_Name");
            DropIndex("dbo.DataProcessingAgreementReadModels", new[] { "SourceEntityId" });
            DropIndex("dbo.DataProcessingAgreementReadModels", new[] { "OrganizationId" });
            DropIndex("dbo.DataProcessingAgreementReadModels", "DataProcessingAgreementReadModel_Index_Name");
            DropTable("dbo.PendingReadModelUpdates");
            DropTable("dbo.DataProcessingAgreements");
            DropTable("dbo.DataProcessingAgreementReadModels");
        }
    }
}
