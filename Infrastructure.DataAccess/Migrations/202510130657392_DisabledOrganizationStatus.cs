namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DisabledOrganizationStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organization", "Disabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Organization", "Disabled");
        }
    }
}
