namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class datatransferedandnameofthirdcountryaddedtoitsystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystem", "IsDataTransferedToThirdCountries", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystem", "DataIsTransferedTo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystem", "DataIsTransferedTo");
            DropColumn("dbo.ItSystem", "IsDataTransferedToThirdCountries");
        }
    }
}
