namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class nameaddedtodataprotectionadvisoranddataResponsibleonorganization : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProtectionAdvisors", "Name", c => c.String());
            AddColumn("dbo.DataResponsibles", "Name", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DataResponsibles", "Name");
            DropColumn("dbo.DataProtectionAdvisors", "Name");
        }
    }
}
