namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep13 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.TerminationDeadlines", newName: "TerminationDeadlineTypes");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.TerminationDeadlineTypes", newName: "TerminationDeadlines");
        }
    }
}
