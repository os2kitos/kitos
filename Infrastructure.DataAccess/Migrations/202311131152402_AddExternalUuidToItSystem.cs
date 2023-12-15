namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExternalUuidToItSystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystem", "ExternalUuid", c => c.Guid());
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "ExternalSystemUuid", c => c.Guid());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "ExternalSystemUuid");
            DropColumn("dbo.ItSystem", "ExternalUuid");
        }
    }
}
