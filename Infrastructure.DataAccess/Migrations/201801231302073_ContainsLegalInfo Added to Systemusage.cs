namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ContainsLegalInfoAddedtoSystemusage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "ContainsLegalInfo", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsage", "ContainsLegalInfo");
        }
    }
}
