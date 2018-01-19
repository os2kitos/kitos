namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dataworkersaddedtosystem : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItSystemDataWorkerRelations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemId = c.Int(nullable: false),
                        DataWorkerId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.DataWorkerId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.ItSystem", t => t.ItSystemId, cascadeDelete: true)
                .Index(t => t.ItSystemId)
                .Index(t => t.DataWorkerId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystemDataWorkerRelations", "ItSystemId", "dbo.ItSystem");
            DropForeignKey("dbo.ItSystemDataWorkerRelations", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ItSystemDataWorkerRelations", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ItSystemDataWorkerRelations", "DataWorkerId", "dbo.Organization");
            DropIndex("dbo.ItSystemDataWorkerRelations", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ItSystemDataWorkerRelations", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ItSystemDataWorkerRelations", new[] { "DataWorkerId" });
            DropIndex("dbo.ItSystemDataWorkerRelations", new[] { "ItSystemId" });
            DropTable("dbo.ItSystemDataWorkerRelations");
        }
    }
}
