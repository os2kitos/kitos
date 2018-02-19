namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArchivingNew : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ArchivePeriod",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        EndDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        UniqueArchiveId = c.String(),
                        ItSystemUsageId = c.Int(nullable: false),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                        ItSystem_Id = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsage", t => t.ItSystemUsageId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.LastChangedByUserId)
                .ForeignKey("dbo.User", t => t.ObjectOwnerId)
                .ForeignKey("dbo.ItSystem", t => t.ItSystem_Id)
                .Index(t => t.ItSystemUsageId)
                .Index(t => t.ObjectOwnerId)
                .Index(t => t.LastChangedByUserId)
                .Index(t => t.ItSystem_Id);
            
            AddColumn("dbo.ItSystem", "ArchiveDuty", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemUsage", "ArchiveSupplier", c => c.String());
            AddColumn("dbo.ItSystemUsage", "SupplierId", c => c.Int());
            AlterColumn("dbo.ItSystemUsage", "ArchiveDuty", c => c.Int());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ArchivePeriod", "ItSystem_Id", "dbo.ItSystem");
            DropForeignKey("dbo.ArchivePeriod", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.ArchivePeriod", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ArchivePeriod", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropIndex("dbo.ArchivePeriod", new[] { "ItSystem_Id" });
            DropIndex("dbo.ArchivePeriod", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ArchivePeriod", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ArchivePeriod", new[] { "ItSystemUsageId" });
            AlterColumn("dbo.ItSystemUsage", "ArchiveDuty", c => c.Boolean());
            DropColumn("dbo.ItSystemUsage", "SupplierId");
            DropColumn("dbo.ItSystemUsage", "ArchiveSupplier");
            DropColumn("dbo.ItSystem", "ArchiveDuty");
            DropTable("dbo.ArchivePeriod");
        }
    }
}
