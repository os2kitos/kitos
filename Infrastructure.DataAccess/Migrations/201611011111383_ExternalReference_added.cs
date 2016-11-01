namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExternalReference_added : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExternalReferences",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        ExternalReferenceId = c.String(),
                        URL = c.String(),
                        Display = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                        ItContract_Id = c.Int(),
                        ItProject_Id = c.Int(),
                        ItSystem_Id = c.Int(),
                        Organization_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.ItContract", t => t.ItContract_Id)
                .ForeignKey("dbo.ItProject", t => t.ItProject_Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id)
                .ForeignKey("dbo.Organization", t => t.Organization_Id)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.ItContract_Id)
                .Index(t => t.ItProject_Id)
                .Index(t => t.ItSystem_Id)
                .Index(t => t.Organization_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExternalReferences", "Organization_Id", "dbo.Organization");
            DropForeignKey("dbo.ExternalReferences", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.ExternalReferences", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.ExternalReferences", "ItContract_Id", "dbo.ItContract");
            DropForeignKey("dbo.ExternalReferences", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ExternalReferences", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.ExternalReferences", new[] { "Organization_Id" });
            DropIndex("dbo.ExternalReferences", new[] { "ItSystem_Id" });
            DropIndex("dbo.ExternalReferences", new[] { "ItProject_Id" });
            DropIndex("dbo.ExternalReferences", new[] { "ItContract_Id" });
            DropIndex("dbo.ExternalReferences", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ExternalReferences", new[] { "ObjectOwnerId" });
            DropTable("dbo.ExternalReferences");
        }
    }
}
