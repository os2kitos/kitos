namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddProcurmentInitiatedToItContract : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContract", "ProcurementInitiated", c => c.Int());
            CreateIndex("dbo.ItContract", "ProcurementInitiated");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItContract", new[] { "ProcurementInitiated" });
            DropColumn("dbo.ItContract", "ProcurementInitiated");
        }
    }
}
