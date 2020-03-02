namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Removed_Wishes : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Wish", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.Wish", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.Wish", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.Wish", "UserId", "dbo.User");
            DropForeignKey("dbo.Wish", "ItSystem_Id", "dbo.ItSystem");
            DropIndex("dbo.Wish", new[] { "UserId" });
            DropIndex("dbo.Wish", new[] { "ItSystemUsageId" });
            DropIndex("dbo.Wish", new[] { "ObjectOwnerId" });
            DropIndex("dbo.Wish", new[] { "LastChangedByUserId" });
            DropIndex("dbo.Wish", new[] { "ItSystem_Id" });
            DropTable("dbo.Wish");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Wish",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IsPublic = c.Boolean(nullable: false),
                        Text = c.String(),
                        UserId = c.Int(nullable: false),
                        ItSystemUsageId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                        ItSystem_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.Wish", "ItSystem_Id");
            CreateIndex("dbo.Wish", "LastChangedByUserId");
            CreateIndex("dbo.Wish", "ObjectOwnerId");
            CreateIndex("dbo.Wish", "ItSystemUsageId");
            CreateIndex("dbo.Wish", "UserId");
            AddForeignKey("dbo.Wish", "ItSystem_Id", "dbo.ItSystem", "Id");
            AddForeignKey("dbo.Wish", "UserId", "dbo.User", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Wish", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.Wish", "LastChangedByUserId", "dbo.User", "Id");
            AddForeignKey("dbo.Wish", "ItSystemUsageId", "dbo.ItSystemUsage", "Id", cascadeDelete: true);
        }
    }
}
