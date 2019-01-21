namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class usersupervisioninfoaddedtosysusage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "UserSupervisionDate", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItSystemUsage", "UserSupervision", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsage", "UserSupervision");
            DropColumn("dbo.ItSystemUsage", "UserSupervisionDate");
        }
    }
}
