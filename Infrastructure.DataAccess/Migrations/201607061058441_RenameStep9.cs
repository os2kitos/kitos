namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep9 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.PaymentModels", newName: "PaymentModelTypes");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.PaymentModelTypes", newName: "PaymentModels");
        }
    }
}
