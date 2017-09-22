namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPreviousNametoItSystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystem", "PreviousName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystem", "PreviousName");
        }
    }
}
