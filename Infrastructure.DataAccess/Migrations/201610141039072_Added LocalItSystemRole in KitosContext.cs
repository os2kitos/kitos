namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedLocalItSystemRoleinKitosContext : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LocalItSystemRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LocalItSystemRoles", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalItSystemRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalItSystemRoles", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.LocalItSystemRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalItSystemRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalItSystemRoles", new[] { "OrganizationId" });
            DropTable("dbo.LocalItSystemRoles");
        }
    }
}
