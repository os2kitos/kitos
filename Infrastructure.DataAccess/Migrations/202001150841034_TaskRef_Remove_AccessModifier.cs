namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskRef_Remove_AccessModifier : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.TaskRef", "UX_AccessModifier");
            DropColumn("dbo.TaskRef", "AccessModifier");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TaskRef", "AccessModifier", c => c.Int(nullable: false));
            CreateIndex("dbo.TaskRef", "AccessModifier", name: "UX_AccessModifier");
        }
    }
}
