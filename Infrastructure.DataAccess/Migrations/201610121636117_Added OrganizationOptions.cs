namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOrganizationOptions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrganizationOptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Description = c.String(),
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
            DropForeignKey("dbo.OrganizationOptions", "Organization_Id", "dbo.Organization");
            DropForeignKey("dbo.OrganizationOptions", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.OrganizationOptions", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.OrganizationOptions", new[] { "Organization_Id" });
            DropIndex("dbo.OrganizationOptions", new[] { "LastChangedByUserId" });
            DropIndex("dbo.OrganizationOptions", new[] { "ObjectOwnerId" });
            DropTable("dbo.OrganizationOptions");
        }
    }
}
