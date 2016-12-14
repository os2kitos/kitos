namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class stopdateaddedtoadvice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Advice", "StopDate", c => c.DateTime(precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Advice", "StopDate");
        }
    }
}
