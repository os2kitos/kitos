namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_Access_Types : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AccessTypes", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystemUsageAccessTypes", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsageAccessTypes", "AccessType_Id", "dbo.AccessTypes");
            DropForeignKey("dbo.AccessTypes", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.AccessTypes", "ObjectOwnerId", "dbo.User");
            DropIndex("dbo.AccessTypes", new[] { "ItSystemId" });
            DropIndex("dbo.AccessTypes", new[] { "ObjectOwnerId" });
            DropIndex("dbo.AccessTypes", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItSystemUsageAccessTypes", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.ItSystemUsageAccessTypes", new[] { "AccessType_Id" });
            DropTable("dbo.AccessTypes");
            DropTable("dbo.ItSystemUsageAccessTypes");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ItSystemUsageAccessTypes",
                c => new
                    {
                        ItSystemUsage_Id = c.Int(nullable: false),
                        AccessType_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItSystemUsage_Id, t.AccessType_Id });
            
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
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.ItSystemUsageAccessTypes", "AccessType_Id");
            CreateIndex("dbo.ItSystemUsageAccessTypes", "ItSystemUsage_Id");
            CreateIndex("dbo.AccessTypes", "LastChangedByUserId");
            CreateIndex("dbo.AccessTypes", "ObjectOwnerId");
            CreateIndex("dbo.AccessTypes", "ItSystemId");
            AddForeignKey("dbo.AccessTypes", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.AccessTypes", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.ItSystemUsageAccessTypes", "AccessType_Id", "dbo.AccessTypes", "Id");
            AddForeignKey("dbo.ItSystemUsageAccessTypes", "ItSystemUsage_Id", "dbo.ItSystemUsage", "Id");
            AddForeignKey("dbo.AccessTypes", "ItSystemId", "dbo.ItSystem", "Id", cascadeDelete: true);
        }
    }
}
