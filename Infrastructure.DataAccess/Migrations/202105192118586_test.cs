namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserNotifications",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        NotificationMessage = c.String(nullable: false, maxLength: 200),
                        RelatedEntityId = c.Int(nullable: false),
                        RelatedEntityType = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserNotifications", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.UserNotifications", "LastChangedByUserId", "dbo.User");
            DropIndex("dbo.UserNotifications", new[] { "LastChangedByUserId" });
            DropIndex("dbo.UserNotifications", new[] { "ObjectOwnerId" });
            DropTable("dbo.UserNotifications");
        }
    }
}
