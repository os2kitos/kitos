namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class notification_uuid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Advice", "Uuid", c => c.Guid(nullable: false));
            AlterColumn("dbo.Advice", "Type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Advice", "Type", c => c.Int());
            DropColumn("dbo.Advice", "Uuid");
        }
    }
}
