namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTask : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TaskRef", "IsPublic", c => c.Boolean(nullable: false));
            AddColumn("dbo.TaskRef", "OwnedByOrganizationUnitId", c => c.Int(nullable: false));
            CreateIndex("dbo.TaskRef", "OwnedByOrganizationUnitId");
            AddForeignKey("dbo.TaskRef", "OwnedByOrganizationUnitId", "dbo.OrganizationUnit", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TaskRef", "OwnedByOrganizationUnitId", "dbo.OrganizationUnit");
            DropIndex("dbo.TaskRef", new[] { "OwnedByOrganizationUnitId" });
            DropColumn("dbo.TaskRef", "OwnedByOrganizationUnitId");
            DropColumn("dbo.TaskRef", "IsPublic");
        }
    }
}
