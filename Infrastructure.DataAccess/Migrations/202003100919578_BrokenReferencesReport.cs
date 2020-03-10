namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BrokenReferencesReport : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BrokenLinkInExternalReferences",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ValueOfCheckedUrl = c.String(),
                        Cause = c.Int(nullable: false),
                        ErrorResponseCode = c.Int(),
                        ReferenceDateOfLatestLinkChange = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        BrokenReferenceOrigin_Id = c.Int(nullable: false),
                        ParentReport_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ExternalReferences", t => t.BrokenReferenceOrigin_Id)
                .ForeignKey("dbo.BrokenExternalReferencesReports", t => t.ParentReport_Id, cascadeDelete: true)
                .Index(t => t.BrokenReferenceOrigin_Id)
                .Index(t => t.ParentReport_Id);
            
            CreateTable(
                "dbo.BrokenExternalReferencesReports",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Created = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BrokenLinkInInterfaces",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ValueOfCheckedUrl = c.String(),
                        Cause = c.Int(nullable: false),
                        ErrorResponseCode = c.Int(),
                        ReferenceDateOfLatestLinkChange = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        BrokenReferenceOrigin_Id = c.Int(nullable: false),
                        ParentReport_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItInterface", t => t.BrokenReferenceOrigin_Id)
                .ForeignKey("dbo.BrokenExternalReferencesReports", t => t.ParentReport_Id, cascadeDelete: true)
                .Index(t => t.BrokenReferenceOrigin_Id)
                .Index(t => t.ParentReport_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BrokenLinkInInterfaces", "ParentReport_Id", "dbo.BrokenExternalReferencesReports");
            DropForeignKey("dbo.BrokenLinkInInterfaces", "BrokenReferenceOrigin_Id", "dbo.ItInterface");
            DropForeignKey("dbo.BrokenLinkInExternalReferences", "ParentReport_Id", "dbo.BrokenExternalReferencesReports");
            DropForeignKey("dbo.BrokenLinkInExternalReferences", "BrokenReferenceOrigin_Id", "dbo.ExternalReferences");
            DropIndex("dbo.BrokenLinkInInterfaces", new[] { "ParentReport_Id" });
            DropIndex("dbo.BrokenLinkInInterfaces", new[] { "BrokenReferenceOrigin_Id" });
            DropIndex("dbo.BrokenLinkInExternalReferences", new[] { "ParentReport_Id" });
            DropIndex("dbo.BrokenLinkInExternalReferences", new[] { "BrokenReferenceOrigin_Id" });
            DropTable("dbo.BrokenLinkInInterfaces");
            DropTable("dbo.BrokenExternalReferencesReports");
            DropTable("dbo.BrokenLinkInExternalReferences");
        }
    }
}
