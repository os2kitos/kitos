namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class RenamedAllTheThings : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "OrganizationRights", newName: "OrganizationUnitRights");
            RenameTable(name: "OrganizationRoles", newName: "OrganizationUnitRoles");
            RenameTable(name: "AdminRights", newName: "OrganizationRights");
            RenameTable(name: "AdminRoles", newName: "OrganizationRoles");
        }

        public override void Down()
        {
            RenameTable(name: "OrganizationUnitRoles", newName: "AdminRoles");
            RenameTable(name: "OrganizationUnitRights", newName: "AdminRights");
            RenameTable(name: "OrganizationRoles", newName: "OrganizationUnitRoles");
            RenameTable(name: "OrganizationRights", newName: "OrganizationUnitRights");
        }
    }
}
