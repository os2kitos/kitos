namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep18 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Methods", newName: "MethodTypes");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.MethodTypes", newName: "Methods");
        }
    }
}
