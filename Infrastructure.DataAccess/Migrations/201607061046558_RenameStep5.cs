namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenameStep5 : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.AdminRoles", newName: "OrganizationRoles");
        }

        public override void Down()
        {
            RenameTable(name: "dbo.OrganizationRoles", newName: "AdminRoles");
        }
    }
}
