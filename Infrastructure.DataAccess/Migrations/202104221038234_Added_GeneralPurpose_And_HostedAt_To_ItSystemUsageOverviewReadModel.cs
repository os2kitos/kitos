namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_GeneralPurpose_And_HostedAt_To_ItSystemUsageOverviewReadModel : DbMigration
    {
        public override void Up()
        {
            RenameIndex(table: "dbo.ItSystemUsageOverviewReadModels", name: "ItSystemUsage_Index_RiskSupervisionDocumentationName", newName: "ItSystemUsageOverviewReadModel_Index_RiskSupervisionDocumentationName");
            RenameIndex(table: "dbo.ItSystemUsageOverviewReadModels", name: "ItSystemUsage_Index_LinkToDirectoryName", newName: "ItSystemUsageOverviewReadModel_Index_LinkToDirectoryName");
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "GeneralPurpose", c => c.String(maxLength: 200));
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "HostedAt", c => c.Int(nullable: false));
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "GeneralPurpose", name: "ItSystemUsageOverviewReadModel_Index_GeneralPurpose");
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "HostedAt", name: "ItSystemUsageOverviewReadModel_Index_HostedAt");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_HostedAt");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_GeneralPurpose");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "HostedAt");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "GeneralPurpose");
            RenameIndex(table: "dbo.ItSystemUsageOverviewReadModels", name: "ItSystemUsageOverviewReadModel_Index_LinkToDirectoryName", newName: "ItSystemUsage_Index_LinkToDirectoryName");
            RenameIndex(table: "dbo.ItSystemUsageOverviewReadModels", name: "ItSystemUsageOverviewReadModel_Index_RiskSupervisionDocumentationName", newName: "ItSystemUsage_Index_RiskSupervisionDocumentationName");
        }
    }
}
