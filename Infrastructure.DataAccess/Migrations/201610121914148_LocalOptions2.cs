namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LocalOptions2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LocalItInterfaceTypes",
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
            DropForeignKey("dbo.LocalItInterfaceTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalItInterfaceTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalItInterfaceTypes", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.LocalItInterfaceTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalItInterfaceTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalItInterfaceTypes", new[] { "OrganizationId" });
            DropTable("dbo.LocalItInterfaceTypes");
        }
    }
}
