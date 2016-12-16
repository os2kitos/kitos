namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ContractSigner : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItContract", "ContractSignerId", "dbo.User");
            DropIndex("dbo.ItContract", new[] { "ContractSignerId" });
            AddColumn("dbo.ItContract", "ContractSigner", c => c.String());
            DropColumn("dbo.ItContract", "ContractSignerId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItContract", "ContractSignerId", c => c.Int());
            DropColumn("dbo.ItContract", "ContractSigner");
            CreateIndex("dbo.ItContract", "ContractSignerId");
            AddForeignKey("dbo.ItContract", "ContractSignerId", "dbo.User", "Id");
        }
    }
}
