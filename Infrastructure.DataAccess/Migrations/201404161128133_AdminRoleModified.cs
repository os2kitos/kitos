namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdminRoleModified : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AdminRole", "HasReadAccess", c => c.Boolean(nullable: false));
            AddColumn("dbo.AdminRole", "HasWriteAccess", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AdminRole", "HasWriteAccess");
            DropColumn("dbo.AdminRole", "HasReadAccess");
        }
    }
}
