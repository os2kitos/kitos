namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Guides : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Config", "ItSupportGuide", c => c.String(unicode: false));
            AddColumn("dbo.Config", "ItProjectGuide", c => c.String(unicode: false));
            AddColumn("dbo.Config", "ItSystemGuide", c => c.String(unicode: false));
            AddColumn("dbo.Config", "ItContractGuide", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Config", "ItContractGuide");
            DropColumn("dbo.Config", "ItSystemGuide");
            DropColumn("dbo.Config", "ItProjectGuide");
            DropColumn("dbo.Config", "ItSupportGuide");
        }
    }
}
