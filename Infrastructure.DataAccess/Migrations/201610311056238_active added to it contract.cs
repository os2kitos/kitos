namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class activeaddedtoitcontract : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContract", "Active", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItContract", "Active");
        }
    }
}
