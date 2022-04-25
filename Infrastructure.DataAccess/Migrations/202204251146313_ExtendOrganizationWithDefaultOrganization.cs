namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Infrastructure.DataAccess.Tools;

    public partial class ExtendOrganizationWithDefaultOrganization : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organization", "IsDefaultOrganization", c => c.Boolean());
            CreateIndex("dbo.Organization", "IsDefaultOrganization", name: "IX_DEFAULT_ORG");
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Set_Default_Organization.sql"));

        }

        public override void Down()
        {
            DropIndex("dbo.Organization", "IX_DEFAULT_ORG");
            DropColumn("dbo.Organization", "IsDefaultOrganization");
        }
    }
}
