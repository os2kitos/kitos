namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep6 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ContractTypes", newName: "ItContractTypes");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.ItContractTypes", newName: "ContractTypes");
        }
    }
}
