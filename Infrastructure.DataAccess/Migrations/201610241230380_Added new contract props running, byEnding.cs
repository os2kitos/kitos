namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddednewcontractpropsrunningbyEnding : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContract", "Running", c => c.String());
            AddColumn("dbo.ItContract", "ByEnding", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItContract", "ByEnding");
            DropColumn("dbo.ItContract", "Running");
        }
    }
}
