namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep23 : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.DataRowUsage", name: "SysUsageId", newName: "ItSystemUsageId");
            RenameColumn(table: "dbo.DataRowUsage", name: "SysId", newName: "ItSystemId");
            RenameColumn(table: "dbo.DataRowUsage", name: "IntfId", newName: "ItInterfaceId");
            RenameIndex(table: "dbo.DataRowUsage", name: "IX_SysUsageId_SysId_IntfId", newName: "IX_ItSystemUsageId_ItSystemId_ItInterfaceId");
        }

        public override void Down()
        {
            RenameIndex(table: "dbo.DataRowUsage", name: "IX_ItSystemUsageId_ItSystemId_ItInterfaceId", newName: "IX_SysUsageId_SysId_IntfId");
            RenameColumn(table: "dbo.DataRowUsage", name: "ItInterfaceId", newName: "IntfId");
            RenameColumn(table: "dbo.DataRowUsage", name: "ItSystemId", newName: "SysId");
            RenameColumn(table: "dbo.DataRowUsage", name: "ItSystemUsageId", newName: "SysUsageId");
        }
    }
}
