namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_UICustomization : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UIModuleCustomizations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrganizationId = c.Int(nullable: false),
                        Module = c.String(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
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
                .ForeignKey("dbo.UIModuleCustomizations", t => t.ModuleId, cascadeDelete: true)
                .Index(t => t.ModuleId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UIModuleCustomizations", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.UIModuleCustomizations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.CustomizedUINodes", "ModuleId", "dbo.UIModuleCustomizations");
            DropForeignKey("dbo.CustomizedUINodes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.CustomizedUINodes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.UIModuleCustomizations", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.CustomizedUINodes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.CustomizedUINodes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.CustomizedUINodes", new[] { "ModuleId" });
            DropIndex("dbo.UIModuleCustomizations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.UIModuleCustomizations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.UIModuleCustomizations", new[] { "OrganizationId" });
            DropTable("dbo.CustomizedUINodes");
            DropTable("dbo.UIModuleCustomizations");
        }
    }
}
