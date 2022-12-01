namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddLatestSubscriptionCheckDateToSts : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StsOrganizationConnections", "DateOfLatestCheckBySubscription", c => c.DateTime(precision: 7, storeType: "datetime2"));
            CreateIndex("dbo.StsOrganizationConnections", "DateOfLatestCheckBySubscription");
        }
        
        public override void Down()
        {
            DropIndex("dbo.StsOrganizationConnections", new[] { "DateOfLatestCheckBySubscription" });
            DropColumn("dbo.StsOrganizationConnections", "DateOfLatestCheckBySubscription");
        }
    }
}
