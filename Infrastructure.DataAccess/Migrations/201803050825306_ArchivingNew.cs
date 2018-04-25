namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArchivingNew : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ArchivePeriod",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        EndDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        UniqueArchiveId = c.String(),
                        ItSystemUsageId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                        ItSystem_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsageId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id)
                .Index(t => t.ItSystemUsageId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.ItSystem_Id);
            
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
            
            AddColumn("dbo.ItSystem", "ArchiveDuty", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemUsage", "ArchiveFreq", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemUsage", "ArchiveSupplier", c => c.String());
            AddColumn("dbo.ItSystemUsage", "Registertype", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemUsage", "SupplierId", c => c.Int());
            AddColumn("dbo.ItSystemUsage", "ArchiveTestLocationId", c => c.Int());
            AlterColumn("dbo.ItSystemUsage", "ArchiveDuty", c => c.Int());
            CreateIndex("dbo.ItSystemUsage", "ArchiveTestLocationId");
            AddForeignKey("dbo.ItSystemUsage", "ArchiveTestLocationId", "dbo.ArchiveTestLocations", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LocalArchiveTestLocations", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalArchiveTestLocations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalArchiveTestLocations", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ArchivePeriod", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystemUsage", "ArchiveTestLocationId", "dbo.ArchiveTestLocations");
            DropForeignKey("dbo.ArchiveTestLocations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ArchiveTestLocations", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ArchivePeriod", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ArchivePeriod", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ArchivePeriod", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropIndex("dbo.LocalArchiveTestLocations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalArchiveTestLocations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalArchiveTestLocations", new[] { "OrganizationId" });
            DropIndex("dbo.ArchiveTestLocations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ArchiveTestLocations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ArchivePeriod", new[] { "ItSystem_Id" });
            DropIndex("dbo.ArchivePeriod", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ArchivePeriod", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ArchivePeriod", new[] { "ItSystemUsageId" });
            DropIndex("dbo.ItSystemUsage", new[] { "ArchiveTestLocationId" });
            AlterColumn("dbo.ItSystemUsage", "ArchiveDuty", c => c.Boolean());
            DropColumn("dbo.ItSystemUsage", "ArchiveTestLocationId");
            DropColumn("dbo.ItSystemUsage", "SupplierId");
            DropColumn("dbo.ItSystemUsage", "Registertype");
            DropColumn("dbo.ItSystemUsage", "ArchiveSupplier");
            DropColumn("dbo.ItSystemUsage", "ArchiveFreq");
            DropColumn("dbo.ItSystem", "ArchiveDuty");
            DropTable("dbo.LocalArchiveTestLocations");
            DropTable("dbo.ArchiveTestLocations");
            DropTable("dbo.ArchivePeriod");
        }
    }
}
