namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class rigsarkiv_fields_on_usage_readmodel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "CatalogArchiveDuty", c => c.Int());
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "CatalogArchiveDutyComment", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "CatalogArchiveDutyComment");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "CatalogArchiveDuty");
        }
    }
}
