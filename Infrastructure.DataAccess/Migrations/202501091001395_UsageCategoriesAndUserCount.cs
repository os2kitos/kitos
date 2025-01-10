namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UsageCategoriesAndUserCount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "ItSystemCategoriesId", c => c.Int());
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "ItSystemCategoriesUuid", c => c.Guid());
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "ItSystemCategoriesName", c => c.String(maxLength: 100));
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "UserCount", c => c.Int(nullable: false));
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemCategoriesId", name: "ItSystemUsageOverviewReadModel_Index_ItSystemCategoriesId");
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemCategoriesUuid", name: "ItSystemUsageOverviewReadModel_Index_ItSystemCategoriesUuid");
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemCategoriesName", name: "ItSystemUsageOverviewReadModel_Index_ItSystemCategoriesName");
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "UserCount", name: "ItSystemUsageOverviewReadModel_Index_UserCount");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_UserCount");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemCategoriesName");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemCategoriesUuid");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemCategoriesId");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "UserCount");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "ItSystemCategoriesName");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "ItSystemCategoriesUuid");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "ItSystemCategoriesId");
        }
    }
}
