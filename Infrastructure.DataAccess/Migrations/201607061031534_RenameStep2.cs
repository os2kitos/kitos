namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep2 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.OrganizationRights", newName: "OrganizationUnitRights");
            RenameTable(name: "dbo.ContractTemplateTypes", newName: "ItContractTemplateTypes");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.ItContractTemplateTypes", newName: "ContractTemplateTypes");
            RenameTable(name: "dbo.OrganizationUnitRights", newName: "OrganizationRights");
        }
    }
}
