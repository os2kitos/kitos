namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Removed_Delegated_SystemUsages : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItSystemUsage", "OrganizationUnit_Id", "dbo.OrganizationUnit");
            DropIndex("dbo.ItSystemUsage", new[] { "OrganizationUnit_Id" });
            DropColumn("dbo.ItSystemUsage", "OrganizationUnit_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsage", "OrganizationUnit_Id", c => c.Int());
            CreateIndex("dbo.ItSystemUsage", "OrganizationUnit_Id");
            AddForeignKey("dbo.ItSystemUsage", "OrganizationUnit_Id", "dbo.OrganizationUnit", "Id");
        }
    }
}
