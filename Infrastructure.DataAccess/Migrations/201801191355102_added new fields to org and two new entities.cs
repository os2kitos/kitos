namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addednewfieldstoorgandtwonewentities : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DataProtectionAdvisors",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Cvr = c.String(maxLength: 10),
                        Phone = c.String(),
                        Adress = c.String(),
                        Email = c.String(),
                        OrganizationId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .Index(t => t.Cvr)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.DataResponsibles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Cvr = c.String(maxLength: 10),
                        Phone = c.String(),
                        Adress = c.String(),
                        Email = c.String(),
                        OrganizationId = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .Index(t => t.Cvr)
                .Index(t => t.OrganizationId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            AddColumn("dbo.Organization", "Phone", c => c.String());
            AddColumn("dbo.Organization", "Adress", c => c.String());
            AddColumn("dbo.Organization", "Email", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataResponsibles", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.DataResponsibles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataResponsibles", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.DataProtectionAdvisors", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.DataProtectionAdvisors", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.DataProtectionAdvisors", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.DataResponsibles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataResponsibles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataResponsibles", new[] { "OrganizationId" });
            DropIndex("dbo.DataResponsibles", new[] { "Cvr" });
            DropIndex("dbo.DataProtectionAdvisors", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataProtectionAdvisors", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataProtectionAdvisors", new[] { "OrganizationId" });
            DropIndex("dbo.DataProtectionAdvisors", new[] { "Cvr" });
            DropColumn("dbo.Organization", "Email");
            DropColumn("dbo.Organization", "Adress");
            DropColumn("dbo.Organization", "Phone");
            DropTable("dbo.DataResponsibles");
            DropTable("dbo.DataProtectionAdvisors");
        }
    }
}
