namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class databehandlereaddedtoitsystemusage : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItSystemUsageDataWorkerRelations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemUsageId = c.Int(nullable: false),
                        DataWorkerId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.DataWorkerId)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsageId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .Index(t => t.ItSystemUsageId)
                .Index(t => t.DataWorkerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystemUsageDataWorkerRelations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItSystemUsageDataWorkerRelations", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemUsageDataWorkerRelations", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsageDataWorkerRelations", "DataWorkerId", "dbo.Organization");
            DropIndex("dbo.ItSystemUsageDataWorkerRelations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItSystemUsageDataWorkerRelations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemUsageDataWorkerRelations", new[] { "DataWorkerId" });
            DropIndex("dbo.ItSystemUsageDataWorkerRelations", new[] { "ItSystemUsageId" });
            DropTable("dbo.ItSystemUsageDataWorkerRelations");
        }
    }
}
