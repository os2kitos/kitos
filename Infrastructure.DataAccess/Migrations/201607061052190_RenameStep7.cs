namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep7 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.OptionExtends", newName: "OptionExtendTypes");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.OptionExtendTypes", newName: "OptionExtends");
        }
    }
}
