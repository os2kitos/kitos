namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_ItSystemUsageOverviewReadModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItSystemUsageOverviewReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrganizationId = c.Int(nullable: false),
                        SourceEntityId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        ItSystemDisabled = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        ParentItSystemName = c.String(maxLength: 100),
                        ParentItSystemId = c.Int(),
                        Version = c.String(maxLength: 100),
                        LocalCallName = c.String(maxLength: 100),
                        LocalSystemId = c.String(maxLength: 100),
                        ItSystemUuid = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .ForeignKey("dbo.ItSystemUsage", t => t.SourceEntityId)
                .Index(t => t.OrganizationId)
                .Index(t => t.SourceEntityId)
                .Index(t => t.Name, name: "ItSystemUsageOverviewReadModel_Index_Name")
                .Index(t => t.ItSystemDisabled, name: "ItSystemUsageOverviewReadModel_Index_ItSystemDisabled")
                .Index(t => t.ParentItSystemName, name: "ItSystemUsageOverviewReadModel_Index_ItSystemParentName")
                .Index(t => t.Version, name: "ItSystemUsageOverviewReadModel_Index_Version")
                .Index(t => t.LocalCallName, name: "ItSystemUsageOverviewReadModel_Index_LocalCallName")
                .Index(t => t.LocalSystemId, name: "ItSystemUsageOverviewReadModel_Index_LocalSystemId")
                .Index(t => t.ItSystemUuid, name: "ItSystemUsageOverviewReadModel_Index_ItSystemUuid");
            
            AlterColumn("dbo.ItSystemUsage", "LocalSystemId", c => c.String(maxLength: 100));
            AlterColumn("dbo.ItSystemUsage", "Version", c => c.String(maxLength: 100));
            AlterColumn("dbo.ItSystemUsage", "LocalCallName", c => c.String(maxLength: 100));
            CreateIndex("dbo.ItSystemUsage", "LocalSystemId", name: "ItSystemUsage_Index_LocalSystemId");
            CreateIndex("dbo.ItSystemUsage", "Version", name: "ItSystemUsage_Index_Version");
            CreateIndex("dbo.ItSystemUsage", "LocalCallName", name: "ItSystemUsage_Index_LocalCallName");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystemUsageOverviewReadModels", "SourceEntityId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsageOverviewReadModels", "OrganizationId", "dbo.Organization");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemUuid");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_LocalSystemId");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_LocalCallName");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_Version");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemParentName");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemDisabled");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_Name");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", new[] { "SourceEntityId" });
            DropIndex("dbo.ItSystemUsageOverviewReadModels", new[] { "OrganizationId" });
            DropIndex("dbo.ItSystemUsage", "ItSystemUsage_Index_LocalCallName");
            DropIndex("dbo.ItSystemUsage", "ItSystemUsage_Index_Version");
            DropIndex("dbo.ItSystemUsage", "ItSystemUsage_Index_LocalSystemId");
            AlterColumn("dbo.ItSystemUsage", "LocalCallName", c => c.String());
            AlterColumn("dbo.ItSystemUsage", "Version", c => c.String());
            AlterColumn("dbo.ItSystemUsage", "LocalSystemId", c => c.String());
            DropTable("dbo.ItSystemUsageOverviewReadModels");
        }
    }
}
