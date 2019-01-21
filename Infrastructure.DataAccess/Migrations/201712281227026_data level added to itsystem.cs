namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dataleveladdedtoitsystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystem", "DataLevel", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystem", "DataLevel");
        }
    }
}
