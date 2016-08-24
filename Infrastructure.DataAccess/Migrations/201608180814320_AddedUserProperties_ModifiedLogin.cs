namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUserProperties_ModifiedLogin : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "LockedOutDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.User", "FailedAttempts", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "FailedAttempts");
            DropColumn("dbo.User", "LockedOutDate");
        }
    }
}
