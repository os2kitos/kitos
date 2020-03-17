namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOperationEnumToItSystemUsage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "operation", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsage", "operation");
        }
    }
}
