namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedOrganizationOptions : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.OrganizationOptions", "Organization_Id", "dbo.Organization");
            DropIndex("dbo.OrganizationOptions", new[] { "Organization_Id" });
            RenameColumn(table: "dbo.OrganizationOptions", name: "Organization_Id", newName: "OrganizationId");
            AddColumn("dbo.OrganizationOptions", "OptionId", c => c.Int(nullable: false));
            AlterColumn("dbo.OrganizationOptions", "OrganizationId", c => c.Int(nullable: false));
            CreateIndex("dbo.OrganizationOptions", "OrganizationId");
            AddForeignKey("dbo.OrganizationOptions", "OrganizationId", "dbo.Organization", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrganizationOptions", "OrganizationId", "dbo.Organization");
            DropIndex("dbo.OrganizationOptions", new[] { "OrganizationId" });
            AlterColumn("dbo.OrganizationOptions", "OrganizationId", c => c.Int());
            DropColumn("dbo.OrganizationOptions", "OptionId");
            RenameColumn(table: "dbo.OrganizationOptions", name: "OrganizationId", newName: "Organization_Id");
            CreateIndex("dbo.OrganizationOptions", "Organization_Id");
            AddForeignKey("dbo.OrganizationOptions", "Organization_Id", "dbo.Organization", "Id");
        }
    }
}
