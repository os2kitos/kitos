namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserDelete : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.User", "EmailBeforeDeletion", c => c.String());
            AddColumn("dbo.User", "DeletedDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.User", "Deleted", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.User", "Deleted");
            DropColumn("dbo.User", "DeletedDate");
            DropColumn("dbo.User", "EmailBeforeDeletion");
        }
    }
}
