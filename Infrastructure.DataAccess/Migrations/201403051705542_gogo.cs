namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class gogo : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContractName", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractName", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractName", "Note", c => c.String(unicode: false));
            AddColumn("dbo.ItProjectName", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectName", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectName", "Note", c => c.String(unicode: false));
            AddColumn("dbo.ItSupportName", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSupportName", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSupportName", "Note", c => c.String(unicode: false));
            AddColumn("dbo.ItSystemName", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemName", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemName", "Note", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ItSystemName", "Note");
            DropColumn("dbo.ItSystemName", "IsSuggestion");
            DropColumn("dbo.ItSystemName", "IsActive");
            DropColumn("dbo.ItSupportName", "Note");
            DropColumn("dbo.ItSupportName", "IsSuggestion");
            DropColumn("dbo.ItSupportName", "IsActive");
            DropColumn("dbo.ItProjectName", "Note");
            DropColumn("dbo.ItProjectName", "IsSuggestion");
            DropColumn("dbo.ItProjectName", "IsActive");
            DropColumn("dbo.ItContractName", "Note");
            DropColumn("dbo.ItContractName", "IsSuggestion");
            DropColumn("dbo.ItContractName", "IsActive");
        }
    }
}
