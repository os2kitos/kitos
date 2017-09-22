namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedLocalOrganizationUnitRolesController : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LocalOrganizationUnitRoles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
                        OrganizationId = c.Int(nullable: false),
                        OptionId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
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
            DropForeignKey("dbo.LocalOrganizationUnitRoles", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.LocalOrganizationUnitRoles", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.LocalOrganizationUnitRoles", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.LocalOrganizationUnitRoles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.LocalOrganizationUnitRoles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.LocalOrganizationUnitRoles", new[] { "OrganizationId" });
            DropTable("dbo.LocalOrganizationUnitRoles");
        }
    }
}
