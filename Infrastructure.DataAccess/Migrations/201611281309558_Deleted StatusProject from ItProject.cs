namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeletedStatusProjectfromItProject : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ItProject", "StatusProject");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItProject", "StatusProject", c => c.Int(nullable: false));
        }
    }
}
