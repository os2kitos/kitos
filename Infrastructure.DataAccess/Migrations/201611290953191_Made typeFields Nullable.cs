namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MadetypeFieldsNullable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Advice", "Scheduling", c => c.Int());
            AlterColumn("dbo.Advice", "Type", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Advice", "Type", c => c.Int(nullable: false));
            DropColumn("dbo.Advice", "Scheduling");
        }
    }
}
