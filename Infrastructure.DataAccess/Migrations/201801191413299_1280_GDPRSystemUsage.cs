namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _1280_GDPRSystemUsage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItSystemCategories",
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
                "dbo.LocalItSystemCategories",
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
            
            AddColumn("dbo.ItSystemUsage", "ItSystemCategoriesId", c => c.Int());
            CreateIndex("dbo.ItSystemUsage", "ItSystemCategoriesId");
            AddForeignKey("dbo.ItSystemUsage", "ItSystemCategoriesId", "dbo.ItSystemCategories", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LocalItSystemCategories", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalItSystemCategories", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalItSystemCategories", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemUsage", "ItSystemCategoriesId", "dbo.ItSystemCategories");
            DropForeignKey("dbo.ItSystemCategories", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItSystemCategories", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.LocalItSystemCategories", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalItSystemCategories", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalItSystemCategories", new[] { "OrganizationId" });
            DropIndex("dbo.ItSystemCategories", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItSystemCategories", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemUsage", new[] { "ItSystemCategoriesId" });
            DropColumn("dbo.ItSystemUsage", "ItSystemCategoriesId");
            DropTable("dbo.LocalItSystemCategories");
            DropTable("dbo.ItSystemCategories");
        }
    }
}
