namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedNullableToUserSupevisionDateInItSystmUsage : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ItSystemUsage", "UserSupervisionDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ItSystemUsage", "UserSupervisionDate", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
        }
    }
}
