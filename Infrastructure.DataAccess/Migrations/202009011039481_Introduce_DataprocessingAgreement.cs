namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Introduce_DataprocessingAgreement : DbMigration
    {
        public override void Up()
        {
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
                .Index(t => t.Name, name: "Contract_Index_Name")
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProcessingAgreements", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.DataProcessingAgreements", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProcessingAgreements", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.DataProcessingAgreements", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataProcessingAgreements", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataProcessingAgreements", new[] { "OrganizationId" });
            DropIndex("dbo.DataProcessingAgreements", "Contract_Index_Name");
            DropTable("dbo.DataProcessingAgreements");
        }
    }
}
