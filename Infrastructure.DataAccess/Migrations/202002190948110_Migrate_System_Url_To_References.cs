namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Migrate_System_Url_To_References : DbMigration
    {
        public override void Up()
        {
            //TODO: Add conversion logic here
            DropColumn("dbo.ItSystem", "Url");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystem", "Url", c => c.String());
        }
    }
}
