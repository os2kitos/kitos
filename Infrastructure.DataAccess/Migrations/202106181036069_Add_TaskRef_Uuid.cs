namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_TaskRef_Uuid : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.TaskRef", "Uuid", unique: true, name: "UX_TaskRef_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.TaskRef", "UX_TaskRef_Uuid");
        }
    }
}
