namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ItSysUsageInfrastrutureChanged : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.InfUsage", "InfrastructureId", "dbo.ItSystem");
            AddForeignKey("dbo.InfUsage", "InfrastructureId", "dbo.ItSystemUsage", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InfUsage", "InfrastructureId", "dbo.ItSystemUsage");
            AddForeignKey("dbo.InfUsage", "InfrastructureId", "dbo.ItSystem", "Id");
        }
    }
}
