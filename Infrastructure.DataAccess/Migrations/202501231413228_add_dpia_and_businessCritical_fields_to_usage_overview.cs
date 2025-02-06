namespace Infrastructure.DataAccess.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class add_dpia_and_businessCritical_fields_to_usage_overview : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "DPIAConducted", c => c.Int());
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "IsBusinessCritical", c => c.Int());
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "DPIAConducted", name: "ItSystemUsageOverviewReadModel_Index_DPIAConducted");
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "IsBusinessCritical", name: "ItSystemUsageOverviewReadModel_Index_IsBusinessCritical");
        }

        public override void Down()
        {
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_IsBusinessCritical");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_DPIAConducted");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "IsBusinessCritical");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "DPIAConducted");
        }
    }
}
