namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Indexes_And_Remove_Default_Organization_Id : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.User", "DefaultOrganizationId", "dbo.Organization");
            DropIndex("dbo.User", new[] { "DefaultOrganizationId" });
            RenameIndex(table: "dbo.User", name: "IX_Email", newName: "User_Index_Email");
            AlterColumn("dbo.User", "Name", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.User", "LastName", c => c.String(maxLength: 100));
            AlterColumn("dbo.ItContract", "Name", c => c.String(nullable: false, maxLength: 200));
            AlterColumn("dbo.ItProject", "Name", c => c.String(nullable: false, maxLength: 150));
            CreateIndex("dbo.User", new[] { "Name", "LastName" }, name: "User_Index_Name");
            CreateIndex("dbo.ItProject", "Name", name: "Project_Index_Name");
            CreateIndex("dbo.ItContract", "Name", name: "Contract_Index_Name");
            DropColumn("dbo.User", "DefaultOrganizationId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.User", "DefaultOrganizationId", c => c.Int());
            DropIndex("dbo.ItContract", "Contract_Index_Name");
            DropIndex("dbo.ItProject", "Project_Index_Name");
            DropIndex("dbo.User", "User_Index_Name");
            AlterColumn("dbo.ItProject", "Name", c => c.String());
            AlterColumn("dbo.ItContract", "Name", c => c.String());
            AlterColumn("dbo.User", "LastName", c => c.String());
            AlterColumn("dbo.User", "Name", c => c.String(nullable: false));
            RenameIndex(table: "dbo.User", name: "User_Index_Email", newName: "IX_Email");
            CreateIndex("dbo.User", "DefaultOrganizationId");
            AddForeignKey("dbo.User", "DefaultOrganizationId", "dbo.Organization", "Id");
        }
    }
}
