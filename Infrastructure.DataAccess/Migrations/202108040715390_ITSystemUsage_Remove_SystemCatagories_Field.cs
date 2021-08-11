namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ITSystemUsage_Remove_SystemCatagories_Field : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ItSystemUsage", "systemCategories");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsage", "systemCategories", c => c.String());
        }
    }
}
