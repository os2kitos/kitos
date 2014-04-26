namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedSystemId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystem", "SystemId", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystem", "SystemId");
        }
    }
}
