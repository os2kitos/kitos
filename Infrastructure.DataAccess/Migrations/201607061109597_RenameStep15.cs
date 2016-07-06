namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep15 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.InterfaceTypes", newName: "ItInterfaceTypes");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.ItInterfaceTypes", newName: "InterfaceTypes");
        }
    }
}
