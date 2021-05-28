namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ItSystemUsageOverviewItSystemUsageReadModel_Rename_ToItSystemUsageOverviewUsedBySystemUsageReadModel : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ItSystemUsageOverviewItSystemUsageReadModels", newName: "ItSystemUsageOverviewUsedBySystemUsageReadModels");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.ItSystemUsageOverviewUsedBySystemUsageReadModels", newName: "ItSystemUsageOverviewItSystemUsageReadModels");
        }
    }
}
