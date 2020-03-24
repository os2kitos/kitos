namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedHostedAtEnumValueToItSystemUsage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "HostedAt", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsage", "HostedAt");
        }
    }
}
