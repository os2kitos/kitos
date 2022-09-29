namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_Expiration_to_itsystemoverview : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "ExpirationDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "LastChangedAt");
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "Concluded");
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "ExpirationDate");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItSystemUsageOverviewReadModels", new[] { "ExpirationDate" });
            DropIndex("dbo.ItSystemUsageOverviewReadModels", new[] { "Concluded" });
            DropIndex("dbo.ItSystemUsageOverviewReadModels", new[] { "LastChangedAt" });
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "ExpirationDate");
        }
    }
}
