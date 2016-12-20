namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Archive : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ArchiveLocations",
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
            
            AddColumn("dbo.ItSystemUsage", "ArchiveDuty", c => c.Boolean());
            AddColumn("dbo.ItSystemUsage", "Archived", c => c.Boolean());
            AddColumn("dbo.ItSystemUsage", "ReportedToDPA", c => c.Boolean());
            AddColumn("dbo.ItSystemUsage", "DocketNo", c => c.String());
            AddColumn("dbo.ItSystemUsage", "ArchivedDate", c => c.DateTime(storeType: "date"));
            AddColumn("dbo.ItSystemUsage", "ArchiveNotes", c => c.String());
            AddColumn("dbo.ItSystemUsage", "ArchiveLocationId", c => c.Int());
            CreateIndex("dbo.ItSystemUsage", "ArchiveLocationId");
            AddForeignKey("dbo.ItSystemUsage", "ArchiveLocationId", "dbo.ArchiveLocations", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LocalArchiveLocations", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalArchiveLocations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalArchiveLocations", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemUsage", "ArchiveLocationId", "dbo.ArchiveLocations");
            DropForeignKey("dbo.ArchiveLocations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ArchiveLocations", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.LocalArchiveLocations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalArchiveLocations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalArchiveLocations", new[] { "OrganizationId" });
            DropIndex("dbo.ArchiveLocations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ArchiveLocations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemUsage", new[] { "ArchiveLocationId" });
            DropColumn("dbo.ItSystemUsage", "ArchiveLocationId");
            DropColumn("dbo.ItSystemUsage", "ArchiveNotes");
            DropColumn("dbo.ItSystemUsage", "ArchivedDate");
            DropColumn("dbo.ItSystemUsage", "DocketNo");
            DropColumn("dbo.ItSystemUsage", "ReportedToDPA");
            DropColumn("dbo.ItSystemUsage", "Archived");
            DropColumn("dbo.ItSystemUsage", "ArchiveDuty");
            DropTable("dbo.LocalArchiveLocations");
            DropTable("dbo.ArchiveLocations");
        }
    }
}
