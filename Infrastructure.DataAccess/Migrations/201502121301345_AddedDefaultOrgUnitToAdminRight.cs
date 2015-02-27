namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedDefaultOrgUnitToAdminRight : DbMigration
    {
        public override void Up()
        {
            AddColumn("AdminRights", "DefaultOrgUnitId", c => c.Int());
            CreateIndex("AdminRights", "DefaultOrgUnitId");
            AddForeignKey("AdminRights", "DefaultOrgUnitId", "OrganizationUnit", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("AdminRights", "DefaultOrgUnitId", "OrganizationUnit");
            DropIndex("AdminRights", new[] { "DefaultOrgUnitId" });
            DropColumn("AdminRights", "DefaultOrgUnitId");
        }
    }
}
