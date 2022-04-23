namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMissingOrganizationDependenciesInEfMap : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DataProtectionAdvisors", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.DataResponsibles", "OrganizationId", "dbo.Organization");
            AddForeignKey("dbo.DataProtectionAdvisors", "OrganizationId", "dbo.Organization", "Id", cascadeDelete: true);
            AddForeignKey("dbo.DataResponsibles", "OrganizationId", "dbo.Organization", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataResponsibles", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.DataProtectionAdvisors", "OrganizationId", "dbo.Organization");
            AddForeignKey("dbo.DataResponsibles", "OrganizationId", "dbo.Organization", "Id");
            AddForeignKey("dbo.DataProtectionAdvisors", "OrganizationId", "dbo.Organization", "Id");
        }
    }
}
