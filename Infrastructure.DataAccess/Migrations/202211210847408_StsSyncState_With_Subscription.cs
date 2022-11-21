namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StsSyncState_With_Subscription : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StsOrganizationConnections", "SubscribeToUpdates", c => c.Boolean(nullable: false));
            CreateIndex("dbo.StsOrganizationConnections", "SubscribeToUpdates", name: "IX_Required");
        }
        
        public override void Down()
        {
            DropIndex("dbo.StsOrganizationConnections", "IX_Required");
            DropColumn("dbo.StsOrganizationConnections", "SubscribeToUpdates");
        }
    }
}
