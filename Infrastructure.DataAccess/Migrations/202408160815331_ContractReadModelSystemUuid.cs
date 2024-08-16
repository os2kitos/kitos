namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ContractReadModelSystemUuid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContractOverviewReadModelItSystemUsages", "ItSystemUsageUuid", c => c.Guid(nullable: false));
            CreateIndex("dbo.ItContractOverviewReadModelItSystemUsages", "ItSystemUsageUuid", name: "IX_ItContract_Read_System_Usage_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItContractOverviewReadModelItSystemUsages", "IX_ItContract_Read_System_Usage_Uuid");
            DropColumn("dbo.ItContractOverviewReadModelItSystemUsages", "ItSystemUsageUuid");
        }
    }
}
