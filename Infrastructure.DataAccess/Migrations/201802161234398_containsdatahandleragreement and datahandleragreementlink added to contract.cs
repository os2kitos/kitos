namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class containsdatahandleragreementanddatahandleragreementlinkaddedtocontract : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContract", "ContainsDataHandlerAgreement", c => c.Int(nullable: false));
            AddColumn("dbo.ItContract", "DataHandlerAgreementUrl", c => c.String());
            AddColumn("dbo.ItContract", "DataHandlerId", c => c.Int());
            CreateIndex("dbo.ItContract", "DataHandlerId");
            AddForeignKey("dbo.ItContract", "DataHandlerId", "dbo.ItContract", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItContract", "DataHandlerId", "dbo.ItContract");
            DropIndex("dbo.ItContract", new[] { "DataHandlerId" });
            DropColumn("dbo.ItContract", "DataHandlerId");
            DropColumn("dbo.ItContract", "DataHandlerAgreementUrl");
            DropColumn("dbo.ItContract", "ContainsDataHandlerAgreement");
        }
    }
}
