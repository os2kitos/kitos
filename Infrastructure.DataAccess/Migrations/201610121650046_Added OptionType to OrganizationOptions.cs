namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedOptionTypetoOrganizationOptions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrganizationOptions", "OptionType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrganizationOptions", "OptionType");
        }
    }
}
