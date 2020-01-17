namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TaskRefCharLimitAndIndexOnTaskKeyIndexOnUuid : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.TaskRef", "TaskKey", c => c.String(maxLength: 50));
            CreateIndex("dbo.TaskRef", "Uuid", unique: true, name: "UX_Uuid");
            CreateIndex("dbo.TaskRef", "TaskKey", unique: true, name: "UX_TaskKey");
        }
        
        public override void Down()
        {
            DropIndex("dbo.TaskRef", "UX_TaskKey");
            DropIndex("dbo.TaskRef", "UX_Uuid");
            AlterColumn("dbo.TaskRef", "TaskKey", c => c.String());
        }
    }
}
