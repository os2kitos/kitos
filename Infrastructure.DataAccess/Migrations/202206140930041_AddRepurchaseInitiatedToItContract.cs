namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddRepurchaseInitiatedToItContract : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContract", "RepurchaseInitiated", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItContract", "RepurchaseInitiated");
        }
    }
}
