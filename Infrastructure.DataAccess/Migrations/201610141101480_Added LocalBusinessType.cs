namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedLocalBusinessType : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LocalBusinessTypes",
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
            DropForeignKey("dbo.LocalBusinessTypes", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalBusinessTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalBusinessTypes", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.LocalBusinessTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalBusinessTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalBusinessTypes", new[] { "OrganizationId" });
            DropTable("dbo.LocalBusinessTypes");
        }
    }
}
