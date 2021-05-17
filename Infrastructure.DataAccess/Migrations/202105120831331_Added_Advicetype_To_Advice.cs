namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Advicetype_To_Advice : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Advice", "AdviceType", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Advice", "AdviceType");
        }
    }
}
