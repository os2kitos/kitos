namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedDisabletoItSystemandItInterface : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystem", "Disabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItInterface", "Disabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItInterface", "Disabled");
            DropColumn("dbo.ItSystem", "Disabled");
        }
    }
}
