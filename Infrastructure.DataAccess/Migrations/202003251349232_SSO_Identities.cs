namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SSO_Identities : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SsoOrganizationIdentities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExternalUuid = c.Guid(nullable: false),
                        Organization_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.Organization_Id, cascadeDelete: true)
                .Index(t => t.ExternalUuid, unique: true, name: "UX_ExternalUuid")
                .Index(t => t.Organization_Id);
            
            CreateTable(
                "dbo.SsoUserIdentities",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExternalUuid = c.Guid(nullable: false),
                        User_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.ExternalUuid, unique: true, name: "UX_ExternalUuid")
                .Index(t => t.User_Id);
            
            DropColumn("dbo.User", "UniqueId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.User", "UniqueId", c => c.String());
            DropForeignKey("dbo.SsoUserIdentities", "User_Id", "dbo.User");
            DropForeignKey("dbo.SsoOrganizationIdentities", "Organization_Id", "dbo.Organization");
            DropIndex("dbo.SsoUserIdentities", new[] { "User_Id" });
            DropIndex("dbo.SsoUserIdentities", "UX_ExternalUuid");
            DropIndex("dbo.SsoOrganizationIdentities", new[] { "Organization_Id" });
            DropIndex("dbo.SsoOrganizationIdentities", "UX_ExternalUuid");
            DropTable("dbo.SsoUserIdentities");
            DropTable("dbo.SsoOrganizationIdentities");
        }
    }
}
