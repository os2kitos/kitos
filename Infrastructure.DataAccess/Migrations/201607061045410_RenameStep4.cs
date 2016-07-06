namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep4 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.AdminRights", newName: "OrganizationRights");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.OrganizationRights", newName: "AdminRights");
        }
    }
}
