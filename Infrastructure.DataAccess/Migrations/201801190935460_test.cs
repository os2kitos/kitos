namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.DataProtectionAdvisors", name: "Organization_Id", newName: "OrganizationId");
            RenameColumn(table: "dbo.DataResponsibles", name: "Organization_Id", newName: "OrganizationId");
            RenameIndex(table: "dbo.DataProtectionAdvisors", name: "IX_Organization_Id", newName: "IX_OrganizationId");
            RenameIndex(table: "dbo.DataResponsibles", name: "IX_Organization_Id", newName: "IX_OrganizationId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.DataResponsibles", name: "IX_OrganizationId", newName: "IX_Organization_Id");
            RenameIndex(table: "dbo.DataProtectionAdvisors", name: "IX_OrganizationId", newName: "IX_Organization_Id");
            RenameColumn(table: "dbo.DataResponsibles", name: "OrganizationId", newName: "Organization_Id");
            RenameColumn(table: "dbo.DataProtectionAdvisors", name: "OrganizationId", newName: "Organization_Id");
        }
    }
}
