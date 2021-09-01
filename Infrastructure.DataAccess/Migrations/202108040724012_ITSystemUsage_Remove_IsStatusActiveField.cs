namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ITSystemUsage_Remove_IsStatusActiveField : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ItSystemUsage", "IsStatusActive");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsage", "IsStatusActive", c => c.Boolean(nullable: false));
        }
    }
}
