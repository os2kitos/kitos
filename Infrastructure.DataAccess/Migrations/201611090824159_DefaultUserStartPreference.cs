namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DefaultUserStartPreference : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "DefaultUserStartPreference", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "DefaultUserStartPreference");
        }
    }
}
