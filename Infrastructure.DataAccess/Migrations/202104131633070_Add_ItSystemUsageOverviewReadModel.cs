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
                        ItSystemParentName = c.String(maxLength: 100),
                        Version = c.String(),
                        LocalCallName = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .ForeignKey("dbo.ItSystemUsage", t => t.SourceEntityId)
                .Index(t => t.OrganizationId)
                .Index(t => t.SourceEntityId)
                .Index(t => t.Name, name: "ItSystemUsageOverviewReadModel_Index_Name")
                .Index(t => t.ItSystemDisabled, name: "ItSystemUsageOverviewReadModel_Index_ItSystemDisabled")
                .Index(t => t.ItSystemParentName, name: "ItSystemUsageOverviewReadModel_Index_ItSystemParentName")
                .Index(t => t.LocalCallName, name: "ItSystemUsageOverviewReadModel_Index_LocalCallName");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystemUsageOverviewReadModels", "SourceEntityId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsageOverviewReadModels", "OrganizationId", "dbo.Organization");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_LocalCallName");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemParentName");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemDisabled");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_Name");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", new[] { "SourceEntityId" });
            DropIndex("dbo.ItSystemUsageOverviewReadModels", new[] { "OrganizationId" });
            DropTable("dbo.ItSystemUsageOverviewReadModels");
        }
    }
}
