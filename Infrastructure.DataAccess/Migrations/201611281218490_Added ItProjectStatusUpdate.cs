namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedItProjectStatusUpdate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItProjectStatusUpdates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AssociatedItProjectId = c.Int(),
                        IsCombined = c.Boolean(nullable: false),
                        Note = c.String(),
                        TimeStatus = c.Int(nullable: false),
                        QualityStatus = c.Int(nullable: false),
                        ResourcesStatus = c.Int(nullable: false),
                        CombinedStatus = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        OrganizationId = c.Int(nullable: false),
                        IsFinal = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItProject", t => t.AssociatedItProjectId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.AssociatedItProjectId)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItProjectStatusUpdates", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItProjectStatusUpdates", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItProjectStatusUpdates", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItProjectStatusUpdates", "AssociatedItProjectId", "dbo.ItProject");
            DropIndex("dbo.ItProjectStatusUpdates", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItProjectStatusUpdates", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItProjectStatusUpdates", new[] { "OrganizationId" });
            DropIndex("dbo.ItProjectStatusUpdates", new[] { "AssociatedItProjectId" });
            DropTable("dbo.ItProjectStatusUpdates");
        }
    }
}
