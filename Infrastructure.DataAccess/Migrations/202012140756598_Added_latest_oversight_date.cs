namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_latest_oversight_date : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrationReadModels", "LatestOversightDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataProcessingRegistrationReadModels", "LatestOversightDate");
        }
    }
}
