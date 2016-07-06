namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep21 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.InfUsage", newName: "ItInterfaceUsage");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.ItInterfaceUsage", newName: "InfUsage");
        }
    }
}
