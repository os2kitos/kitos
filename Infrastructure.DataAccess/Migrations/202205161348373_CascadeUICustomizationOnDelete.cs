namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CascadeUICustomizationOnDelete : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CustomizedUINodes", "ModuleId", "dbo.UIModuleCustomizations");
            DropForeignKey("dbo.UIModuleCustomizations", "OrganizationId", "dbo.Organization");
            AddForeignKey("dbo.CustomizedUINodes", "ModuleId", "dbo.UIModuleCustomizations", "Id", cascadeDelete: true);
            AddForeignKey("dbo.UIModuleCustomizations", "OrganizationId", "dbo.Organization", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UIModuleCustomizations", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.CustomizedUINodes", "ModuleId", "dbo.UIModuleCustomizations");
            AddForeignKey("dbo.UIModuleCustomizations", "OrganizationId", "dbo.Organization", "Id");
            AddForeignKey("dbo.CustomizedUINodes", "ModuleId", "dbo.UIModuleCustomizations", "Id");
        }
    }
}
