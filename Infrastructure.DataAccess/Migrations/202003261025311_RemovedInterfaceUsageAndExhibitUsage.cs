namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemovedInterfaceUsageAndExhibitUsage : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItInterfaceExhibitUsage", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.ItInterfaceExhibitUsage", "ItInterfaceExhibitId", "dbo.Exhibit");
            DropForeignKey("dbo.ItInterfaceExhibitUsage", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItInterfaceUsage", "InfrastructureId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItInterfaceUsage", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.ItInterfaceUsage", "ItInterfaceId", "dbo.ItInterface");
            DropForeignKey("dbo.ItInterfaceUsage", "ItSystemUsageId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItInterfaceExhibitUsage", "ItInterface_Id", "dbo.ItInterface");
            DropIndex("dbo.ItInterfaceExhibitUsage", new[] { "ItSystemUsageId" });
            DropIndex("dbo.ItInterfaceExhibitUsage", new[] { "ItInterfaceExhibitId" });
            DropIndex("dbo.ItInterfaceExhibitUsage", new[] { "ItContractId" });
            DropIndex("dbo.ItInterfaceExhibitUsage", new[] { "ItInterface_Id" });
            DropIndex("dbo.ItInterfaceUsage", new[] { "ItSystemUsageId" });
            DropIndex("dbo.ItInterfaceUsage", new[] { "ItInterfaceId" });
            DropIndex("dbo.ItInterfaceUsage", new[] { "ItContractId" });
            DropIndex("dbo.ItInterfaceUsage", new[] { "InfrastructureId" });
            DropTable("dbo.ItInterfaceExhibitUsage");
            DropTable("dbo.ItInterfaceUsage");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ItInterfaceUsage",
                c => new
                    {
                        ItSystemUsageId = c.Int(nullable: false),
                        ItSystemId = c.Int(nullable: false),
                        ItInterfaceId = c.Int(nullable: false),
                        ItContractId = c.Int(),
                        InfrastructureId = c.Int(),
                        IsWishedFor = c.Boolean(nullable: false),
                        MigratedToUuid = c.Guid(),
                    })
                .PrimaryKey(t => new { t.ItSystemUsageId, t.ItSystemId, t.ItInterfaceId });
            
            CreateTable(
                "dbo.ItInterfaceExhibitUsage",
                c => new
                    {
                        ItSystemUsageId = c.Int(nullable: false),
                        ItInterfaceExhibitId = c.Int(nullable: false),
                        ItContractId = c.Int(),
                        IsWishedFor = c.Boolean(nullable: false),
                        ItInterface_Id = c.Int(),
                    })
                .PrimaryKey(t => new { t.ItSystemUsageId, t.ItInterfaceExhibitId });
            
            CreateIndex("dbo.ItInterfaceUsage", "InfrastructureId");
            CreateIndex("dbo.ItInterfaceUsage", "ItContractId");
            CreateIndex("dbo.ItInterfaceUsage", "ItInterfaceId");
            CreateIndex("dbo.ItInterfaceUsage", "ItSystemUsageId");
            CreateIndex("dbo.ItInterfaceExhibitUsage", "ItInterface_Id");
            CreateIndex("dbo.ItInterfaceExhibitUsage", "ItContractId");
            CreateIndex("dbo.ItInterfaceExhibitUsage", "ItInterfaceExhibitId");
            CreateIndex("dbo.ItInterfaceExhibitUsage", "ItSystemUsageId");
            AddForeignKey("dbo.ItInterfaceExhibitUsage", "ItInterface_Id", "dbo.ItInterface", "Id");
            AddForeignKey("dbo.ItInterfaceUsage", "ItSystemUsageId", "dbo.ItSystemUsage", "Id");
            AddForeignKey("dbo.ItInterfaceUsage", "ItInterfaceId", "dbo.ItInterface", "Id");
            AddForeignKey("dbo.ItInterfaceUsage", "ItContractId", "dbo.ItContract", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItInterfaceUsage", "InfrastructureId", "dbo.ItSystemUsage", "Id");
            AddForeignKey("dbo.ItInterfaceExhibitUsage", "ItSystemUsageId", "dbo.ItSystemUsage", "Id");
            AddForeignKey("dbo.ItInterfaceExhibitUsage", "ItInterfaceExhibitId", "dbo.Exhibit", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ItInterfaceExhibitUsage", "ItContractId", "dbo.ItContract", "Id");
        }
    }
}
