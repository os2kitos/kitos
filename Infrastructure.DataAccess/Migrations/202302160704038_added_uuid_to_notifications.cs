namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class added_uuid_to_notifications : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Advice", "Uuid", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Advice", "Uuid");
        }
    }
}
