namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UIModuleIndex : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.UIModuleCustomizations", new[] { "OrganizationId" });
            AlterColumn("dbo.UIModuleCustomizations", "Module", c => c.String(nullable: false, maxLength: 200));
            CreateIndex("dbo.UIModuleCustomizations", new[] { "OrganizationId", "Module" }, unique: true, name: "UX_OrganizationIdModuleUnique");
        }
        
        public override void Down()
        {
            DropIndex("dbo.UIModuleCustomizations", "UX_OrganizationIdModuleUnique");
            AlterColumn("dbo.UIModuleCustomizations", "Module", c => c.String(nullable: false));
            CreateIndex("dbo.UIModuleCustomizations", "OrganizationId");
        }
    }
}
