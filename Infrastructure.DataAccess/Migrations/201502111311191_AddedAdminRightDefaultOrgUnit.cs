namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedAdminRightDefaultOrgUnit : DbMigration
    {
        public override void Up()
        {
            AddColumn("AdminRights", "DefaultOrgUnitId", c => c.Int());
            CreateIndex("AdminRights", "DefaultOrgUnitId");
            AddForeignKey("AdminRights", "DefaultOrgUnitId", "Organization", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("AdminRights", "DefaultOrgUnitId", "Organization");
            DropIndex("AdminRights", new[] { "DefaultOrgUnitId" });
            DropColumn("AdminRights", "DefaultOrgUnitId");
        }
    }
}
