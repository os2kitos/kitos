namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Extend_ItContractOverviewReadModel_With_Name : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContractOverviewReadModels", "Name", c => c.String(maxLength: 200));
            CreateIndex("dbo.ItContractOverviewReadModels", "Name", name: "IX_Contract_Name");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItContractOverviewReadModels", "IX_Contract_Name");
            DropColumn("dbo.ItContractOverviewReadModels", "Name");
        }
    }
}
