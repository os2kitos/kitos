namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep12 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.PurchaseForms", newName: "PurchaseFormTypes");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.PurchaseFormTypes", newName: "PurchaseForms");
        }
    }
}
