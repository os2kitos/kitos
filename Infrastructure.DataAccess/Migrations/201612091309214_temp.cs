namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class temp : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AccessTypes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        ItSystemId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ItSystemId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
            CreateTable(
                "dbo.ItSystemUsageAccessTypes",
                c => new
                    {
                        ItSystemUsage_Id = c.Int(nullable: false),
                        AccessType_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItSystemUsage_Id, t.AccessType_Id })
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsage_Id)
                .ForeignKey("dbo.AccessTypes", t => t.AccessType_Id)
                .Index(t => t.ItSystemUsage_Id)
                .Index(t => t.AccessType_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AccessTypes", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.AccessTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemUsageAccessTypes", "AccessType_Id", "dbo.AccessTypes");
            DropForeignKey("dbo.ItSystemUsageAccessTypes", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.AccessTypes", "ItSystemId", "dbo.ItSystem");
            DropIndex("dbo.ItSystemUsageAccessTypes", new[] { "AccessType_Id" });
            DropIndex("dbo.ItSystemUsageAccessTypes", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.AccessTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.AccessTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.AccessTypes", new[] { "ItSystemId" });
            DropTable("dbo.ItSystemUsageAccessTypes");
            DropTable("dbo.AccessTypes");
        }
    }
}
