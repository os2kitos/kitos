namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Xauth : DbMigration
    {
        public override void Up()
        {
            AddColumn("User", "Uuid", c => c.Guid());
        }
        
        public override void Down()
        {
            DropColumn("User", "Uuid");
        }
    }
}
