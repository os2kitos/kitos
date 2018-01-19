namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newentitiesadded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataProtectionAdvisors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Cvr = c.Int(nullable: false),
                        Phone = c.String(),
                        Adress = c.String(),
                        Email = c.String(),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.DataResponsibles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Cvr = c.Int(nullable: false),
                        Phone = c.String(),
                        Adress = c.String(),
                        Email = c.String(),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                        Organization_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.Organization_Id)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.Organization_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataResponsibles", "Organization_Id", "dbo.Organization");
            DropForeignKey("dbo.DataResponsibles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataResponsibles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataProtectionAdvisors", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProtectionAdvisors", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.DataResponsibles", new[] { "Organization_Id" });
            DropIndex("dbo.DataResponsibles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataResponsibles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataProtectionAdvisors", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataProtectionAdvisors", new[] { "ObjectOwnerId" });
            DropTable("dbo.DataResponsibles");
            DropTable("dbo.DataProtectionAdvisors");
        }
    }
}
