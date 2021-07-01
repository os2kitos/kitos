namespace Infrastructure.DataAccess.Migrations
{
    using Infrastructure.DataAccess.Tools;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Uuid_To_ItProject : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItProject", "Uuid", c => c.Guid(nullable: false));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Patch_Uuid_Project.sql"));
            CreateIndex("dbo.ItProject", "Uuid", unique: true, name: "UX_Project_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItProject", "UX_Project_Uuid");
            DropColumn("dbo.ItProject", "Uuid");
        }
    }
}
