namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep16 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Interfaces", newName: "InterfaceTypes");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.InterfaceTypes", newName: "Interfaces");
        }
    }
}
