namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrganizationUnitRemoveTaskRefsProperty : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TaskRef", "OrganizationUnit_Id", "dbo.OrganizationUnit");
            DropIndex("dbo.TaskRef", new[] { "OrganizationUnit_Id" });
            DropColumn("dbo.TaskRef", "OrganizationUnit_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TaskRef", "OrganizationUnit_Id", c => c.Int());
            CreateIndex("dbo.TaskRef", "OrganizationUnit_Id");
            AddForeignKey("dbo.TaskRef", "OrganizationUnit_Id", "dbo.OrganizationUnit", "Id");
        }
    }
}
