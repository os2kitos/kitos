namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DefaultUserPreferenceaddedtoUserentity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "DefaultUserPreference", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "DefaultUserPreference");
        }
    }
}
