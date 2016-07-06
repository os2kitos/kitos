namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep19 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Tsas", newName: "TsaTypes");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.TsaTypes", newName: "Tsas");
        }
    }
}
