namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Relation_UUID : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SystemRelations", "Uuid", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SystemRelations", "Uuid");
        }
    }
}
