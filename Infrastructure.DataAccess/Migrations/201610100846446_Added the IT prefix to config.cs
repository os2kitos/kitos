namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedtheITprefixtoconfig : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Config", "ShowItProjectPrefix", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.Config", "ShowItSystemPrefix", c => c.Boolean(nullable: false, defaultValue: false));
            AddColumn("dbo.Config", "ShowItContractPrefix", c => c.Boolean(nullable: false, defaultValue: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Config", "ShowItContractPrefix");
            DropColumn("dbo.Config", "ShowItSystemPrefix");
            DropColumn("dbo.Config", "ShowItProjectPrefix");
        }
    }
}
