using System.Linq;

namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Migrate_System_Url_To_References : DbMigration
    {
        public override void Up()
        {
            //Migrate before changing the schema
            SqlResource(GetType().Assembly.GetManifestResourceNames().First(x => x.Contains("Migrate_System_Url.sql")));

            DropColumn("dbo.ItSystem", "Url");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystem", "Url", c => c.String());
        }
    }
}
