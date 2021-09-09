namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ItContract_Cleanup : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ItContract", "Esdh");
            DropColumn("dbo.ItContract", "Folder");
            DropColumn("dbo.ItContract", "OperationTestExpected");
            DropColumn("dbo.ItContract", "OperationTestApproved");
            DropColumn("dbo.ItContract", "OperationalAcceptanceTestExpected");
            DropColumn("dbo.ItContract", "OperationalAcceptanceTestApproved");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItContract", "OperationalAcceptanceTestApproved", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItContract", "OperationalAcceptanceTestExpected", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItContract", "OperationTestApproved", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItContract", "OperationTestExpected", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.ItContract", "Folder", c => c.String());
            AddColumn("dbo.ItContract", "Esdh", c => c.String());
        }
    }
}
