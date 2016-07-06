namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep17 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ItSystemTypeOptions", newName: "ItSystemTypes");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.ItSystemTypes", newName: "ItSystemTypeOptions");
        }
    }
}
