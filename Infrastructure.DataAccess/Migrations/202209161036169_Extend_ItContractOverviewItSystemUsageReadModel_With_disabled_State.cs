namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Extend_ItContractOverviewItSystemUsageReadModel_With_disabled_State : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContractOverviewReadModelItSystemUsages", "ItSystemIsDisabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItContractOverviewReadModelItSystemUsages", "ItSystemIsDisabled");
        }
    }
}
