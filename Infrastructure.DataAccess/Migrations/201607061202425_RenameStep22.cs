namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep22 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.OrgUnitSystemUsage", newName: "ItSystemUsageOrganizationUnits");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.ItSystemUsageOrganizationUnits", newName: "OrgUnitSystemUsage");
        }
    }
}
