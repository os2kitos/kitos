namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Extend_SystemUsageOverviewReadmodel_With_Contracts : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItSystemUsageOverviewItContractReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItContractUuid = c.Guid(nullable: false),
                        ItContractId = c.Int(nullable: false),
                        ItContractName = c.String(nullable: false, maxLength: 200),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsageOverviewReadModels", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.ItContractId, name: "ItContractId")
                .Index(t => t.ItContractName, name: "ItContractNameName")
                .Index(t => t.ParentId);
            
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "AssociatedContractsNamesCsv", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystemUsageOverviewItContractReadModels", "ParentId", "dbo.ItSystemUsageOverviewReadModels");
            DropIndex("dbo.ItSystemUsageOverviewItContractReadModels", new[] { "ParentId" });
            DropIndex("dbo.ItSystemUsageOverviewItContractReadModels", "ItContractNameName");
            DropIndex("dbo.ItSystemUsageOverviewItContractReadModels", "ItContractId");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "AssociatedContractsNamesCsv");
            DropTable("dbo.ItSystemUsageOverviewItContractReadModels");
        }
    }
}
