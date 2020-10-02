namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Dpr_Rename_Country_References_Table : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.DataProcessingRegistrations", "DataProcessingCountryOption_Id", "dbo.DataProcessingCountryOptions");
            DropIndex("dbo.DataProcessingRegistrations", new[] { "DataProcessingCountryOption_Id" });
            DropColumn("dbo.DataProcessingRegistrations", "DataProcessingCountryOption_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DataProcessingRegistrations", "DataProcessingCountryOption_Id", c => c.Int());
            CreateIndex("dbo.DataProcessingRegistrations", "DataProcessingCountryOption_Id");
            AddForeignKey("dbo.DataProcessingRegistrations", "DataProcessingCountryOption_Id", "dbo.DataProcessingCountryOptions", "Id");
        }
    }
}
