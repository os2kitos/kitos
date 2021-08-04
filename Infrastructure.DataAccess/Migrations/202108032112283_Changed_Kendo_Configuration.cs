namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Changed_Kendo_Configuration : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.KendoOrganizationalConfigurations", "Version", c => c.Int(nullable: false));
            AddColumn("dbo.KendoOrganizationalConfigurations", "VisibleColumnsCsv", c => c.String());
            DropColumn("dbo.KendoOrganizationalConfigurations", "Configuration");
        }
        
        public override void Down()
        {
            AddColumn("dbo.KendoOrganizationalConfigurations", "Configuration", c => c.String(nullable: false));
            DropColumn("dbo.KendoOrganizationalConfigurations", "VisibleColumnsCsv");
            DropColumn("dbo.KendoOrganizationalConfigurations", "Version");
        }
    }
}
