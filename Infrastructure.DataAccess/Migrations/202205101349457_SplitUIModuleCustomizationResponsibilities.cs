namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SplitUIModuleCustomizationResponsibilities : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CustomizedUINodes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ModuleId = c.Int(nullable: false),
                        Key = c.String(nullable: false),
                        Enabled = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.UIModuleCustomizations", t => t.ModuleId)
                .Index(t => t.ModuleId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            DropColumn("dbo.UIModuleCustomizations", "Key");
            DropColumn("dbo.UIModuleCustomizations", "Enabled");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UIModuleCustomizations", "Enabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.UIModuleCustomizations", "Key", c => c.String(nullable: false));
            DropForeignKey("dbo.CustomizedUINodes", "ModuleId", "dbo.UIModuleCustomizations");
            DropForeignKey("dbo.CustomizedUINodes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.CustomizedUINodes", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.CustomizedUINodes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.CustomizedUINodes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.CustomizedUINodes", new[] { "ModuleId" });
            DropTable("dbo.CustomizedUINodes");
        }
    }
}
