namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UsageReadModelSystemDescription : DbMigration
    {
        public override void Up()
        {
            RenameIndex(table: "dbo.ItSystemUsageOverviewReadModels", name: "ItSystemUsageOverviewReadModel_Index_Name", newName: "ItSystemUsageOverviewReadModel_Index_System_Description");
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "SystemDescription", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "SystemDescription");
            RenameIndex(table: "dbo.ItSystemUsageOverviewReadModels", name: "ItSystemUsageOverviewReadModel_Index_System_Description", newName: "ItSystemUsageOverviewReadModel_Index_Name");
        }
    }
}
