namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GDPRChanges : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystem", "LinkToDirectoryAdminUrl", c => c.String());
            AddColumn("dbo.ItSystem", "LinkToDirectoryAdminUrlName", c => c.String());
            AddColumn("dbo.ItSystemUsage", "precautionsOptionsEncryption", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemUsage", "precautionsOptionsPseudonomisering", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemUsage", "precautionsOptionsAccessControl", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemUsage", "precautionsOptionsLogning", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemUsage", "LinkToDirectoryUrl", c => c.String());
            AddColumn("dbo.ItSystemUsage", "LinkToDirectoryUrlName", c => c.String());
            DropColumn("dbo.ItSystemUsage", "precautionsOptions");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsage", "precautionsOptions", c => c.Int(nullable: false));
            DropColumn("dbo.ItSystemUsage", "LinkToDirectoryUrlName");
            DropColumn("dbo.ItSystemUsage", "LinkToDirectoryUrl");
            DropColumn("dbo.ItSystemUsage", "precautionsOptionsLogning");
            DropColumn("dbo.ItSystemUsage", "precautionsOptionsAccessControl");
            DropColumn("dbo.ItSystemUsage", "precautionsOptionsPseudonomisering");
            DropColumn("dbo.ItSystemUsage", "precautionsOptionsEncryption");
            DropColumn("dbo.ItSystem", "LinkToDirectoryAdminUrlName");
            DropColumn("dbo.ItSystem", "LinkToDirectoryAdminUrl");
        }
    }
}
