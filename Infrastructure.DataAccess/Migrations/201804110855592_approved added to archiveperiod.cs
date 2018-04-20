namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class approvedaddedtoarchiveperiod : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ArchivePeriod", "Approved", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ArchivePeriod", "Approved");
        }
    }
}
