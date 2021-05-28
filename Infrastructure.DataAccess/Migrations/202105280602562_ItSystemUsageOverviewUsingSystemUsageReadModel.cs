namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ItSystemUsageOverviewUsingSystemUsageReadModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItSystemUsageOverviewUsingSystemUsageReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemUsageId = c.Int(nullable: false),
                        ItSystemUsageName = c.String(nullable: false, maxLength: 100),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsageOverviewReadModels", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.ItSystemUsageId, name: "ItSystemUsageOverviewUsingSystemUsageReadModel_index_ItSystemUsageId")
                .Index(t => t.ItSystemUsageName, name: "ItSystemUsageOverviewUsingSystemUsageReadModel_index_ItSystemUsageName")
                .Index(t => t.ParentId);
            
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "OutgoingRelatedItSystemUsagesNamesAsCsv", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystemUsageOverviewUsingSystemUsageReadModels", "ParentId", "dbo.ItSystemUsageOverviewReadModels");
            DropIndex("dbo.ItSystemUsageOverviewUsingSystemUsageReadModels", new[] { "ParentId" });
            DropIndex("dbo.ItSystemUsageOverviewUsingSystemUsageReadModels", "ItSystemUsageOverviewUsingSystemUsageReadModel_index_ItSystemUsageName");
            DropIndex("dbo.ItSystemUsageOverviewUsingSystemUsageReadModels", "ItSystemUsageOverviewUsingSystemUsageReadModel_index_ItSystemUsageId");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "OutgoingRelatedItSystemUsagesNamesAsCsv");
            DropTable("dbo.ItSystemUsageOverviewUsingSystemUsageReadModels");
        }
    }
}
