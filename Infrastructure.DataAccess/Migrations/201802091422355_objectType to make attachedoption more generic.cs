namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class objectTypetomakeattachedoptionmoregeneric : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AttachedOptions", "ObjectType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AttachedOptions", "ObjectType");
        }
    }
}
