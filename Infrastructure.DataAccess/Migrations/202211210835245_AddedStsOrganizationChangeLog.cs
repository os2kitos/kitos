namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedStsOrganizationChangeLog : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StsOrganizationChangeLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ConnectionId = c.Int(nullable: false),
                        Origin = c.Int(nullable: false),
                        Name = c.String(),
                        LogTime = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.StsOrganizationConnections", t => t.ConnectionId, cascadeDelete: true)
                .Index(t => t.ConnectionId)
                .Index(t => t.Origin, name: "UX_ChangeLogOrigin")
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.StsOrganizationConsequenceLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ChangeLogId = c.Int(nullable: false),
                        Uuid = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        Type = c.Int(nullable: false),
                        Description = c.String(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.StsOrganizationChangeLogs", t => t.ChangeLogId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ChangeLogId)
                .Index(t => t.Uuid, name: "UX_Consequence_Uuid")
                .Index(t => t.Type, name: "UX_Consequence_Type")
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StsOrganizationChangeLogs", "ConnectionId", "dbo.StsOrganizationConnections");
            DropForeignKey("dbo.StsOrganizationChangeLogs", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.StsOrganizationChangeLogs", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.StsOrganizationConsequenceLogs", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.StsOrganizationConsequenceLogs", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.StsOrganizationConsequenceLogs", "ChangeLogId", "dbo.StsOrganizationChangeLogs");
            DropIndex("dbo.StsOrganizationConsequenceLogs", new[] { "LastChangedByUserId" });
            DropIndex("dbo.StsOrganizationConsequenceLogs", new[] { "ObjectOwnerId" });
            DropIndex("dbo.StsOrganizationConsequenceLogs", "UX_Consequence_Type");
            DropIndex("dbo.StsOrganizationConsequenceLogs", "UX_Consequence_Uuid");
            DropIndex("dbo.StsOrganizationConsequenceLogs", new[] { "ChangeLogId" });
            DropIndex("dbo.StsOrganizationChangeLogs", new[] { "LastChangedByUserId" });
            DropIndex("dbo.StsOrganizationChangeLogs", new[] { "ObjectOwnerId" });
            DropIndex("dbo.StsOrganizationChangeLogs", "UX_ChangeLogOrigin");
            DropIndex("dbo.StsOrganizationChangeLogs", new[] { "ConnectionId" });
            DropTable("dbo.StsOrganizationConsequenceLogs");
            DropTable("dbo.StsOrganizationChangeLogs");
        }
    }
}
