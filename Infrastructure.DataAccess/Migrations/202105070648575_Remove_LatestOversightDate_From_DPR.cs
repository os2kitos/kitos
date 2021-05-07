namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_LatestOversightDate_From_DPR : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.DataProcessingRegistrations", "LatestOversightDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.DataProcessingRegistrations", "LatestOversightDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
        }
    }
}
