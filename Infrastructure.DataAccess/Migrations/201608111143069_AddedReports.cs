namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedReports : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Reports",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        CategoryTypeId = c.Int(),
                        OrganizationId = c.Int(nullable: false),
                        Definition = c.String(),
                        AccessModifier = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ReportCategoryTypes", t => t.CategoryTypeId)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .Index(t => t.CategoryTypeId)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ReportCategoryTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Reports", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.Reports", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Reports", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Reports", "CategoryTypeId", "dbo.ReportCategoryTypes");
            DropForeignKey("dbo.ReportCategoryTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ReportCategoryTypes", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.ReportCategoryTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ReportCategoryTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Reports", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Reports", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Reports", new[] { "OrganizationId" });
            DropIndex("dbo.Reports", new[] { "CategoryTypeId" });
            DropTable("dbo.ReportCategoryTypes");
            DropTable("dbo.Reports");
        }
    }
}
