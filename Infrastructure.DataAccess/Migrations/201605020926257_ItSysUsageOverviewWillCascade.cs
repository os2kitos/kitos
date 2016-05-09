namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class ItSysUsageOverviewWillCascade : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("ItSystemUsage", "OverviewId", "ItSystemUsage");
            AddForeignKey("ItSystemUsage", "OverviewId", "ItSystemUsage", "Id", cascadeDelete: true);
        }

        public override void Down()
        {
            DropForeignKey("ItSystemUsage", "OverviewId", "ItSystemUsage");
            AddForeignKey("ItSystemUsage", "OverviewId", "ItSystemUsage", "Id");
        }
    }
}
