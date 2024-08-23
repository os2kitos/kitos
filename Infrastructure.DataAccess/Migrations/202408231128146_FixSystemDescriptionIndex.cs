namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixSystemDescriptionIndex : DbMigration
    {
        public override void Up()
        {
            RenameIndex(table: "dbo.ItSystemUsageOverviewReadModels", name: "ItSystemUsageOverviewReadModel_Index_System_Description", newName: "ItSystemUsageOverviewReadModel_Index_Name");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.ItSystemUsageOverviewReadModels", name: "ItSystemUsageOverviewReadModel_Index_Name", newName: "ItSystemUsageOverviewReadModel_Index_System_Description");
        }
    }
}
