namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep24 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.itusageorgusage", newName: "ItSystemUsageOrgUnitUsages");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.ItSystemUsageOrgUnitUsages", newName: "itusageorgusage");
        }
    }
}
