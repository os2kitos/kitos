namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserLastAdvisDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("User", "LastAdvisDate", c => c.DateTime(precision: 0));
        }
        
        public override void Down()
        {
            DropColumn("User", "LastAdvisDate");
        }
    }
}
