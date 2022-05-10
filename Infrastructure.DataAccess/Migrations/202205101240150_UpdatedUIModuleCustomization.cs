namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdatedUIModuleCustomization : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.UIVisibilityConfigurations", newName: "UIModuleCustomizations");
            AlterColumn("dbo.UIModuleCustomizations", "Enabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UIModuleCustomizations", "Enabled", c => c.String(nullable: false));
            RenameTable(name: "dbo.UIModuleCustomizations", newName: "UIVisibilityConfigurations");
        }
    }
}
