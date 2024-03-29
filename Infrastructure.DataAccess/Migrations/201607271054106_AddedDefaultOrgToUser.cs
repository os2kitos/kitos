namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddedDefaultOrgToUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "DefaultOrganizationId", c => c.Int());
            CreateIndex("dbo.User", "DefaultOrganizationId");
            AddForeignKey("dbo.User", "DefaultOrganizationId", "dbo.Organization", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.User", "DefaultOrganizationId", "dbo.Organization");
            DropIndex("dbo.User", new[] { "DefaultOrganizationId" });
            DropColumn("dbo.User", "DefaultOrganizationId");
        }
    }
}
