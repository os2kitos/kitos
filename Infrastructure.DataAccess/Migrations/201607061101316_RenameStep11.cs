namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep11 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.ProcurementStrategies", newName: "ProcurementStrategyTypes");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.ProcurementStrategyTypes", newName: "ProcurementStrategies");
        }
    }
}
