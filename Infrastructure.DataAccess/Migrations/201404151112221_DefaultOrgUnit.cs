namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DefaultOrgUnit : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "DefaultOrganizationUnitId", c => c.Int());
            CreateIndex("dbo.User", "DefaultOrganizationUnitId");
            AddForeignKey("dbo.User", "DefaultOrganizationUnitId", "dbo.OrganizationUnit", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.User", "DefaultOrganizationUnitId", "dbo.OrganizationUnit");
            DropIndex("dbo.User", new[] { "DefaultOrganizationUnitId" });
            DropColumn("dbo.User", "DefaultOrganizationUnitId");
        }
    }
}
