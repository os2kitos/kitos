namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedKeytoHelpText : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.HelpTexts", "Key", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.HelpTexts", "Key");
        }
    }
}
