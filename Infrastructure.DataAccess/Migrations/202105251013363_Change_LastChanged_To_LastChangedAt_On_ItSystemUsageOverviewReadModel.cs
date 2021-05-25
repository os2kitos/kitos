namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Change_LastChanged_To_LastChangedAt_On_ItSystemUsageOverviewReadModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "LastChangedAt", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "LastChanged");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "LastChanged", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "LastChangedAt");
        }
    }
}
