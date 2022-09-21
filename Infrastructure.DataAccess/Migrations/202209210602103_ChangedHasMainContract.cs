using System.Data.Entity.Migrations;

namespace Infrastructure.DataAccess.Migrations
{
    public partial class ChangedHasMainContract : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_HasMainContract");
            AlterColumn("dbo.ItSystemUsageOverviewReadModels", "MainContractIsActive", c => c.Boolean(nullable: false));
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "HasMainContract");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "HasMainContract", c => c.Boolean(nullable: false));
            AlterColumn("dbo.ItSystemUsageOverviewReadModels", "MainContractIsActive", c => c.Boolean());
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "HasMainContract", name: "ItSystemUsageOverviewReadModel_Index_HasMainContract");
        }
    }
}
