namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class generalpurposeaddedtoitsystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystem", "GeneralPurpose", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystem", "GeneralPurpose");
        }
    }
}
