namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SystemOverview_Add_Relevant_Org_Units : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItSystemUsageOverviewRelevantOrgUnitReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrganizationUnitUuid = c.Guid(nullable: false),
                        OrganizationUnitId = c.Int(nullable: false),
                        OrganizationUnitName = c.String(nullable: false, maxLength: 100),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsageOverviewReadModels", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.OrganizationUnitUuid, name: "IX_OrgUnitUuid")
                .Index(t => t.OrganizationUnitId, name: "IX_OrgUnitId")
                .Index(t => t.OrganizationUnitName, name: "IX_Name")
                .Index(t => t.ParentId);
            
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "RelevantOrganizationUnitNamesAsCsv", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystemUsageOverviewRelevantOrgUnitReadModels", "ParentId", "dbo.ItSystemUsageOverviewReadModels");
            DropIndex("dbo.ItSystemUsageOverviewRelevantOrgUnitReadModels", new[] { "ParentId" });
            DropIndex("dbo.ItSystemUsageOverviewRelevantOrgUnitReadModels", "IX_Name");
            DropIndex("dbo.ItSystemUsageOverviewRelevantOrgUnitReadModels", "IX_OrgUnitId");
            DropIndex("dbo.ItSystemUsageOverviewRelevantOrgUnitReadModels", "IX_OrgUnitUuid");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "RelevantOrganizationUnitNamesAsCsv");
            DropTable("dbo.ItSystemUsageOverviewRelevantOrgUnitReadModels");
        }
    }
}
