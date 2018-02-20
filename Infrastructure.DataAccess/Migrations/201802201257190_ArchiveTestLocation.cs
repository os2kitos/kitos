namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArchiveTestLocation : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ArchiveTestLocations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        IsLocallyAvailable = c.Boolean(nullable: false),
                        IsObligatory = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Description = c.String(),
                        IsEnabled = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.LocalArchiveTestLocations",
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
            
            AddColumn("dbo.ItSystemUsage", "ArchiveFreq", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemUsage", "Registertype", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemUsage", "ArchiveTestLocationId", c => c.Int());
            CreateIndex("dbo.ItSystemUsage", "ArchiveTestLocationId");
            AddForeignKey("dbo.ItSystemUsage", "ArchiveTestLocationId", "dbo.ArchiveTestLocations", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LocalArchiveTestLocations", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalArchiveTestLocations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalArchiveTestLocations", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemUsage", "ArchiveTestLocationId", "dbo.ArchiveTestLocations");
            DropForeignKey("dbo.ArchiveTestLocations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ArchiveTestLocations", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.LocalArchiveTestLocations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalArchiveTestLocations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalArchiveTestLocations", new[] { "OrganizationId" });
            DropIndex("dbo.ArchiveTestLocations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ArchiveTestLocations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemUsage", new[] { "ArchiveTestLocationId" });
            DropColumn("dbo.ItSystemUsage", "ArchiveTestLocationId");
            DropColumn("dbo.ItSystemUsage", "Registertype");
            DropColumn("dbo.ItSystemUsage", "ArchiveFreq");
            DropTable("dbo.LocalArchiveTestLocations");
            DropTable("dbo.ArchiveTestLocations");
        }
    }
}
