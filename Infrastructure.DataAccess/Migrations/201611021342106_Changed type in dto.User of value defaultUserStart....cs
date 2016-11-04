namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedtypeindtoUserofvaluedefaultUserStart : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "DefaultUserStartPreference", c => c.String());
            DropColumn("dbo.User", "DefaultUserPreference");
        }
        
        public override void Down()
        {
            AddColumn("dbo.User", "DefaultUserPreference", c => c.Int(nullable: false));
            DropColumn("dbo.User", "DefaultUserStartPreference");
        }
    }
}
