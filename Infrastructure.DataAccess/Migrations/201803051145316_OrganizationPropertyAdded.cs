namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrganizationPropertyAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organization", "ForeignCvr", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Organization", "ForeignCvr");
        }
    }
}
