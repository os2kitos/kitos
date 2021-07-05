namespace Infrastructure.DataAccess.Migrations
{
    using Infrastructure.DataAccess.Tools;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Uuid_To_ItProject_And_Removed_AccessModifier_And_Other_Unused_Fields : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItProject", "OriginalId", "dbo.ItProject");
            DropIndex("dbo.ItProject", new[] { "OriginalId" });
            DropIndex("dbo.ItProject", "UX_AccessModifier");
            AddColumn("dbo.ItProject", "Uuid", c => c.Guid(nullable: false));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Patch_Uuid_Project.sql"));
            CreateIndex("dbo.ItProject", "Uuid", unique: true, name: "UX_Project_Uuid");
            DropColumn("dbo.ItProject", "OriginalId");
            DropColumn("dbo.ItProject", "AccessModifier");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItProject", "AccessModifier", c => c.Int(nullable: false));
            AddColumn("dbo.ItProject", "OriginalId", c => c.Int());
            DropIndex("dbo.ItProject", "UX_Project_Uuid");
            DropColumn("dbo.ItProject", "Uuid");
            CreateIndex("dbo.ItProject", "AccessModifier", name: "UX_AccessModifier");
            CreateIndex("dbo.ItProject", "OriginalId");
            AddForeignKey("dbo.ItProject", "OriginalId", "dbo.ItProject", "Id");
        }
    }
}
