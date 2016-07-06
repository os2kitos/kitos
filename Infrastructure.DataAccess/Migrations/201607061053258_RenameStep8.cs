namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep8 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.PaymentFreqencies", newName: "PaymentFreqencyTypes");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.PaymentFreqencyTypes", newName: "PaymentFreqencies");
        }
    }
}
