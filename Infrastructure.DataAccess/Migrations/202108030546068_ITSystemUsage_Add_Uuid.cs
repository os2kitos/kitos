namespace Infrastructure.DataAccess.Migrations
{
    using Infrastructure.DataAccess.Tools;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ITSystemUsage_Add_Uuid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "Uuid", c => c.Guid(nullable: false));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Patch_Uuid_ItSystemUsage.sql"));
            CreateIndex("dbo.ItSystemUsage", "Uuid", unique: true, name: "UX_ItSystemUsage_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItSystemUsage", "UX_ItSystemUsage_Uuid");
            DropColumn("dbo.ItSystemUsage", "Uuid");
        }
    }
}
