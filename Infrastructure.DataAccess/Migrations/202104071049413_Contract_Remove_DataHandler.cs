namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Contract_Remove_DataHandler : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItContract", "DataHandlerId", "dbo.ItContract");
            DropIndex("dbo.ItContract", new[] { "DataHandlerId" });
            DropColumn("dbo.ItContract", "DataHandlerId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItContract", "DataHandlerId", c => c.Int());
            CreateIndex("dbo.ItContract", "DataHandlerId");
            AddForeignKey("dbo.ItContract", "DataHandlerId", "dbo.ItContract", "Id");
        }
    }
}
