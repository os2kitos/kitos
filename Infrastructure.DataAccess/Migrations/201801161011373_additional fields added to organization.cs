namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class additionalfieldsaddedtoorganization : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organization", "Phone", c => c.String());
            AddColumn("dbo.Organization", "Adress", c => c.String());
            AddColumn("dbo.Organization", "Email", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Organization", "Email");
            DropColumn("dbo.Organization", "Adress");
            DropColumn("dbo.Organization", "Phone");
        }
    }
}
