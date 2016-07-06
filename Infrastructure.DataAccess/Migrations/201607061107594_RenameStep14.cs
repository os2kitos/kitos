namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep14 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Frequencies", newName: "FrequencyTypes");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.FrequencyTypes", newName: "Frequencies");
        }
    }
}
