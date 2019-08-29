namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedHasApiAccessBooleanToUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "HasApiAccess", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "HasApiAccess");
        }
    }
}
