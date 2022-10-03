namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Organization_Unit_Origin_Info : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.OrganizationUnit", "UX_LocalId");
            CreateTable(
                "dbo.StsOrganizationConnections",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                        Connected = c.Boolean(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.Id)
                .Index(t => t.Id)
                .Index(t => t.Connected)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            AddColumn("dbo.OrganizationUnit", "Origin", c => c.Int(nullable: false));
            AddColumn("dbo.OrganizationUnit", "ExternalOriginUuid", c => c.Guid());
            CreateIndex("dbo.OrganizationUnit", "ExternalOriginUuid", name: "IX_OrganizationUnit_UUID");
            CreateIndex("dbo.OrganizationUnit", "LocalId", unique: true, name: "UX_LocalId");
            CreateIndex("dbo.OrganizationUnit", "OrganizationId", name: "IX_OrganizationUnit_Origin");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StsOrganizationConnections", "Id", "dbo.Organization");
            DropForeignKey("dbo.StsOrganizationConnections", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.StsOrganizationConnections", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.StsOrganizationConnections", new[] { "LastChangedByUserId" });
            DropIndex("dbo.StsOrganizationConnections", new[] { "ObjectOwnerId" });
            DropIndex("dbo.StsOrganizationConnections", new[] { "Connected" });
            DropIndex("dbo.StsOrganizationConnections", new[] { "Id" });
            DropIndex("dbo.OrganizationUnit", "IX_OrganizationUnit_Origin");
            DropIndex("dbo.OrganizationUnit", "UX_LocalId");
            DropIndex("dbo.OrganizationUnit", "IX_OrganizationUnit_UUID");
            DropColumn("dbo.OrganizationUnit", "ExternalOriginUuid");
            DropColumn("dbo.OrganizationUnit", "Origin");
            DropTable("dbo.StsOrganizationConnections");
            CreateIndex("dbo.OrganizationUnit", new[] { "OrganizationId", "LocalId" }, unique: true, name: "UX_LocalId");
        }
    }
}
