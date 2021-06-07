namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_UserNotifications : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserNotifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        NotificationMessage = c.String(nullable: false),
                        NotificationType = c.Int(nullable: false),
                        NotificationRecipientId = c.Int(nullable: false),
                        Created = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        OrganizationId = c.Int(nullable: false),
                        ItProject_Id = c.Int(),
                        Itcontract_Id = c.Int(),
                        ItSystemUsage_Id = c.Int(),
                        DataProcessingRegistration_Id = c.Int(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DataProcessingRegistrations", t => t.DataProcessingRegistration_Id)
                .ForeignKey("dbo.ItContract", t => t.Itcontract_Id)
                .ForeignKey("dbo.ItProject", t => t.ItProject_Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.NotificationRecipientId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .Index(t => t.NotificationRecipientId)
                .Index(t => t.OrganizationId)
                .Index(t => t.ItProject_Id)
                .Index(t => t.Itcontract_Id)
                .Index(t => t.ItSystemUsage_Id)
                .Index(t => t.DataProcessingRegistration_Id)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserNotifications", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.UserNotifications", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.UserNotifications", "NotificationRecipientId", "dbo.User");
            DropForeignKey("dbo.UserNotifications", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.UserNotifications", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.UserNotifications", "ItProject_Id", "dbo.ItProject");
            DropForeignKey("dbo.UserNotifications", "Itcontract_Id", "dbo.ItContract");
            DropForeignKey("dbo.UserNotifications", "DataProcessingRegistration_Id", "dbo.DataProcessingRegistrations");
            DropIndex("dbo.UserNotifications", new[] { "LastChangedByUserId" });
            DropIndex("dbo.UserNotifications", new[] { "ObjectOwnerId" });
            DropIndex("dbo.UserNotifications", new[] { "DataProcessingRegistration_Id" });
            DropIndex("dbo.UserNotifications", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.UserNotifications", new[] { "Itcontract_Id" });
            DropIndex("dbo.UserNotifications", new[] { "ItProject_Id" });
            DropIndex("dbo.UserNotifications", new[] { "OrganizationId" });
            DropIndex("dbo.UserNotifications", new[] { "NotificationRecipientId" });
            DropTable("dbo.UserNotifications");
        }
    }
}
