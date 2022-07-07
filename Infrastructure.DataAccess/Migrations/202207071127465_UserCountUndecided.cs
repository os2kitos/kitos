namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserCountUndecided : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE dbo.ItSystemUsage
                  SET UserCount = null 
                  WHERE UserCount = 0"
            );
            AlterColumn("dbo.ItSystemUsage", "UserCount", c => c.Int());
        }
        
        public override void Down()
        {
            Sql(@"UPDATE dbo.ItSystemUsage
                  SET UserCount = 0 
                  WHERE UserCount IS NULL"
            );
            AlterColumn("dbo.ItSystemUsage", "UserCount", c => c.Int(nullable: false));
        }
    }
}
