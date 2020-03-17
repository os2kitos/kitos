namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedHostedAtEnumToItSystemUsage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "hostedAt", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsage", "hostedAt");
        }
    }
}
