namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updatedadviceobjecttobemoregeneric : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Advice", "CarbonCopyReceiverId", "dbo.ItContractRoles");
            DropForeignKey("dbo.Advice", "ItContractId", "dbo.ItContract");
            DropForeignKey("dbo.Advice", "ReceiverId", "dbo.ItContractRoles");
            DropIndex("dbo.Advice", new[] { "ReceiverId" });
            DropIndex("dbo.Advice", new[] { "CarbonCopyReceiverId" });
            DropIndex("dbo.Advice", new[] { "ItContractId" });
            AddColumn("dbo.Advice", "RelationId", c => c.Int());
            AddColumn("dbo.Advice", "Type", c => c.Int(nullable: false));
            DropColumn("dbo.Advice", "ItContractId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Advice", "ItContractId", c => c.Int(nullable: false));
            DropColumn("dbo.Advice", "Type");
            DropColumn("dbo.Advice", "RelationId");
            CreateIndex("dbo.Advice", "ItContractId");
            CreateIndex("dbo.Advice", "CarbonCopyReceiverId");
            CreateIndex("dbo.Advice", "ReceiverId");
            AddForeignKey("dbo.Advice", "ReceiverId", "dbo.ItContractRoles", "Id");
            AddForeignKey("dbo.Advice", "ItContractId", "dbo.ItContract", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Advice", "CarbonCopyReceiverId", "dbo.ItContractRoles", "Id");
        }
    }
}
