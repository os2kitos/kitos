namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_DataProcessingRegistrations_To_Contracts : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrations", "ItContract_Id", c => c.Int());
            CreateIndex("dbo.DataProcessingRegistrations", "ItContract_Id");
            AddForeignKey("dbo.DataProcessingRegistrations", "ItContract_Id", "dbo.ItContract", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProcessingRegistrations", "ItContract_Id", "dbo.ItContract");
            DropIndex("dbo.DataProcessingRegistrations", new[] { "ItContract_Id" });
            DropColumn("dbo.DataProcessingRegistrations", "ItContract_Id");
        }
    }
}
