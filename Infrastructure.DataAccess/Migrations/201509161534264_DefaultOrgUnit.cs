namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class DefaultOrgUnit : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("User", "DefaultOrganizationUnitId", "OrganizationUnit");
            DropIndex("User", new[] { "DefaultOrganizationUnitId" });
            DropColumn("User", "DefaultOrganizationUnitId");
        }

        public override void Down()
        {
            AddColumn("User", "DefaultOrganizationUnitId", c => c.Int());
            CreateIndex("User", "DefaultOrganizationUnitId");
            AddForeignKey("User", "DefaultOrganizationUnitId", "OrganizationUnit");
        }
    }
}
