namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddedNavigationPropToModel : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "itusageorgusage", name: "ItSystemUsage_Id", newName: "ResponsibleItSystemUsage_Id");
            RenameColumn(table: "ItProjectOrgUnitUsages", name: "ItProject_Id", newName: "ResponsibleItProject_Id");
            // MySQL connector doesn't support RenameIndex so doing it manually... thanks MySQL
            Sql("ALTER TABLE ItProjectOrgUnitUsages RENAME INDEX IX_ItProject_Id TO IX_ResponsibleItProject_Id");
            Sql("ALTER TABLE itusageorgusage RENAME INDEX IX_ItSystemUsage_Id TO IX_ResponsibleItSystemUsage_Id");
        }

        public override void Down()
        {
            // MySQL connector doesn't support RenameIndex so doing it manually... thanks MySQL
            Sql("ALTER TABLE itusageorgusage RENAME INDEX IX_ResponsibleItSystemUsage_Id TO IX_ItSystemUsage_Id");
            Sql("ALTER TABLE ItProjectOrgUnitUsages RENAME INDEX IX_ResponsibleItProject_Id TO IX_ItProject_Id");
            RenameColumn(table: "ItProjectOrgUnitUsages", name: "ResponsibleItProject_Id", newName: "ItProject_Id");
            RenameColumn(table: "itusageorgusage", name: "ResponsibleItSystemUsage_Id", newName: "ItSystemUsage_Id");
        }
    }
}
