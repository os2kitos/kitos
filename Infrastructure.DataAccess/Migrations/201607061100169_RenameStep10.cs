namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep10 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.PriceRegulations", newName: "PriceRegulationTypes");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.PriceRegulationTypes", newName: "PriceRegulations");
        }
    }
}
