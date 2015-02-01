namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedUserLastNameAndPhoneNumber : DbMigration
    {
        public override void Up()
        {
            AddColumn("User", "LastName", c => c.String(unicode: false));
            AddColumn("User", "PhoneNumber", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("User", "PhoneNumber");
            DropColumn("User", "LastName");
        }
    }
}
