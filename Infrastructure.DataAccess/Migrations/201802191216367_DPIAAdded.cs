namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DPIAAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "DPIA", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemUsage", "DPIADateFor", c => c.DateTime(precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemUsage", "DPIADateFor");
            DropColumn("dbo.ItSystemUsage", "DPIA");
        }
    }
}
