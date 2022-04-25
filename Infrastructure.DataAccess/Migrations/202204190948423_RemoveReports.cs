namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using Infrastructure.DataAccess.Tools;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveReports : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ReportCategoryTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ReportCategoryTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Reports", "CategoryTypeId", "dbo.ReportCategoryTypes");
            DropForeignKey("dbo.Reports", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Reports", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Reports", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.GlobalConfigs", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.GlobalConfigs", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalReportCategoryTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.LocalReportCategoryTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalReportCategoryTypes", "OrganizationId", "dbo.Organization");
            DropIndex("dbo.Reports", new[] { "CategoryTypeId" });
            DropIndex("dbo.Reports", new[] { "OrganizationId" });
            DropIndex("dbo.Reports", "UX_AccessModifier");
            DropIndex("dbo.Reports", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Reports", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ReportCategoryTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ReportCategoryTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.GlobalConfigs", new[] { "ObjectOwnerId" });
            DropIndex("dbo.GlobalConfigs", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalReportCategoryTypes", new[] { "OrganizationId" });
            DropIndex("dbo.LocalReportCategoryTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalReportCategoryTypes", new[] { "LastChangedByUserId" });
            DropTable("dbo.Reports");
            DropTable("dbo.ReportCategoryTypes");
            DropTable("dbo.GlobalConfigs");
            DropTable("dbo.LocalReportCategoryTypes");
            SqlResource(SqlMigrationScriptRepository.GetResourceName("RemoveReports.sql"));
        }

        public override void Down()
        {
            CreateTable(
                "dbo.LocalReportCategoryTypes",
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
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.GlobalConfigs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        key = c.String(),
                        value = c.String(),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ReportCategoryTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        IsLocallyAvailable = c.Boolean(nullable: false),
                        IsObligatory = c.Boolean(nullable: false),
                        Description = c.String(),
                        IsEnabled = c.Boolean(nullable: false),
                        Priority = c.Int(nullable: false),
                        Uuid = c.Guid(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
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
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.LocalReportCategoryTypes", "LastChangedByUserId");
            CreateIndex("dbo.LocalReportCategoryTypes", "ObjectOwnerId");
            CreateIndex("dbo.LocalReportCategoryTypes", "OrganizationId");
            CreateIndex("dbo.GlobalConfigs", "LastChangedByUserId");
            CreateIndex("dbo.GlobalConfigs", "ObjectOwnerId");
            CreateIndex("dbo.ReportCategoryTypes", "LastChangedByUserId");
            CreateIndex("dbo.ReportCategoryTypes", "ObjectOwnerId");
            CreateIndex("dbo.Reports", "LastChangedByUserId");
            CreateIndex("dbo.Reports", "ObjectOwnerId");
            CreateIndex("dbo.Reports", "AccessModifier", name: "UX_AccessModifier");
            CreateIndex("dbo.Reports", "OrganizationId");
            CreateIndex("dbo.Reports", "CategoryTypeId");
            AddForeignKey("dbo.LocalReportCategoryTypes", "OrganizationId", "dbo.Organization", "Id", cascadeDelete: true);
            AddForeignKey("dbo.LocalReportCategoryTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.LocalReportCategoryTypes", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.GlobalConfigs", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.GlobalConfigs", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.Reports", "OrganizationId", "dbo.Organization", "Id");
            AddForeignKey("dbo.Reports", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.Reports", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.Reports", "CategoryTypeId", "dbo.ReportCategoryTypes", "Id");
            AddForeignKey("dbo.ReportCategoryTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.ReportCategoryTypes", "LastChangedByUserId", "dbo.User", "Id");
        }
    }
}
