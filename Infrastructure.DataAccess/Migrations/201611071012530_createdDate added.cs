namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class createdDateadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ExternalReferences", "Created", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExternalReferences", "Created");
        }
    }
}
