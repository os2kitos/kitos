namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class containslegalinfoaddedtoitsystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystem", "ContainsLegalInfo", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystem", "ContainsLegalInfo");
        }
    }
}
