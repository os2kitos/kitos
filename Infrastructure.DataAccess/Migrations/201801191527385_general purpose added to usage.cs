namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class generalpurposeaddedtousage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "GeneralPurpose", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsage", "GeneralPurpose");
        }
    }
}
