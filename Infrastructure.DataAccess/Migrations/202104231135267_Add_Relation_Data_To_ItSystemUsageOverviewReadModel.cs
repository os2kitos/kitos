namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Relation_Data_To_ItSystemUsageOverviewReadModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItSystemUsageOverviewInterfaceReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InterfaceId = c.Int(nullable: false),
                        InterfaceName = c.String(nullable: false, maxLength: 100),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsageOverviewReadModels", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.InterfaceId, name: "ItSystemUsageOverviewInterfaceReadModel_index_InterfaceId")
                .Index(t => t.InterfaceName, name: "ItSystemUsageOverviewInterfaceReadModel_index_InterfaceName")
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.ItSystemUsageOverviewItSystemUsageReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemUsageId = c.Int(nullable: false),
                        ItSystemUsageName = c.String(nullable: false, maxLength: 100),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsageOverviewReadModels", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.ItSystemUsageId, name: "ItSystemUsageOverviewItSystemUsageReadModel_index_ItSystemUsageId")
                .Index(t => t.ItSystemUsageName, name: "ItSystemUsageOverviewItSystemUsageReadModel_index_ItSystemUsageName")
                .Index(t => t.ParentId);
            
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "AppliedInterfacesNamesAsCsv", c => c.String());
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "IncomingRelatedItSystemUsagesNamesAsCsv", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystemUsageOverviewItSystemUsageReadModels", "ParentId", "dbo.ItSystemUsageOverviewReadModels");
            DropForeignKey("dbo.ItSystemUsageOverviewInterfaceReadModels", "ParentId", "dbo.ItSystemUsageOverviewReadModels");
            DropIndex("dbo.ItSystemUsageOverviewItSystemUsageReadModels", new[] { "ParentId" });
            DropIndex("dbo.ItSystemUsageOverviewItSystemUsageReadModels", "ItSystemUsageOverviewItSystemUsageReadModel_index_ItSystemUsageName");
            DropIndex("dbo.ItSystemUsageOverviewItSystemUsageReadModels", "ItSystemUsageOverviewItSystemUsageReadModel_index_ItSystemUsageId");
            DropIndex("dbo.ItSystemUsageOverviewInterfaceReadModels", new[] { "ParentId" });
            DropIndex("dbo.ItSystemUsageOverviewInterfaceReadModels", "ItSystemUsageOverviewInterfaceReadModel_index_InterfaceName");
            DropIndex("dbo.ItSystemUsageOverviewInterfaceReadModels", "ItSystemUsageOverviewInterfaceReadModel_index_InterfaceId");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "IncomingRelatedItSystemUsagesNamesAsCsv");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "AppliedInterfacesNamesAsCsv");
            DropTable("dbo.ItSystemUsageOverviewItSystemUsageReadModels");
            DropTable("dbo.ItSystemUsageOverviewInterfaceReadModels");
        }
    }
}
