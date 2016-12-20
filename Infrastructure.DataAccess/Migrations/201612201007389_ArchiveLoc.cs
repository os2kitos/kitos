namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArchiveLoc : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LocalArchiveLocations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LocalArchiveLocations", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalArchiveLocations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalArchiveLocations", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.LocalArchiveLocations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalArchiveLocations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalArchiveLocations", new[] { "OrganizationId" });
            DropTable("dbo.LocalArchiveLocations");
        }
    }
}
