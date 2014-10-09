namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InfraToSysUsage : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("InfUsage", "InfrastructureId", "ItSystem");
            AddForeignKey("InfUsage", "InfrastructureId", "ItSystemUsage", "Id");
        }

        public override void Down()
        {
            DropForeignKey("InfUsage", "InfrastructureId", "ItSystemUsage");
            AddForeignKey("InfUsage", "InfrastructureId", "ItSystem", "Id");
        }
    }
}
