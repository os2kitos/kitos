namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DataRowFieldsToNullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("DataRowUsage", "Amount", c => c.Int());
            AlterColumn("DataRowUsage", "Economy", c => c.Int());
            AlterColumn("DataRowUsage", "Price", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("DataRowUsage", "Price", c => c.Int(nullable: false));
            AlterColumn("DataRowUsage", "Economy", c => c.Int(nullable: false));
            AlterColumn("DataRowUsage", "Amount", c => c.Int(nullable: false));
        }
    }
}
