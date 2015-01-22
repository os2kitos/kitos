namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserAddIsLocked : DbMigration
    {
        public override void Up()
        {
            AddColumn("User", "IsLocked", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("User", "IsLocked");
        }
    }
}
