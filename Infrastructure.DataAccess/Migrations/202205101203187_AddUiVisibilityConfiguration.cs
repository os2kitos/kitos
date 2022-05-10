namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUiVisibilityConfiguration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UIVisibilityConfigurations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrganizationId = c.Int(nullable: false),
                        Module = c.String(nullable: false),
                        Key = c.String(nullable: false),
                        Enabled = c.String(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UIVisibilityConfigurations", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.UIVisibilityConfigurations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.UIVisibilityConfigurations", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.UIVisibilityConfigurations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.UIVisibilityConfigurations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.UIVisibilityConfigurations", new[] { "OrganizationId" });
            DropTable("dbo.UIVisibilityConfigurations");
        }
    }
}
