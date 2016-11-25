namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mailBodyaddedtoadvice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Advice", "Body", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Advice", "Body");
        }
    }
}
