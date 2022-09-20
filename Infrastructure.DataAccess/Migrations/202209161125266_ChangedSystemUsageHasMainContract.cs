namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
<<<<<<<< HEAD:Infrastructure.DataAccess/Migrations/202209161125266_ChangedSystemUsageHasMainContract.cs
    public partial class ChangedSystemUsageHasMainContract : DbMigration
========
    public partial class ChangedHasMainContract : DbMigration
>>>>>>>> feature/KITOSUDV-3212-merge-fix:Infrastructure.DataAccess/Migrations/202209200853095_ChangedHasMainContract.cs
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
