namespace Infrastructure.DataAccess.Migrations
{
    using Infrastructure.DataAccess.Tools;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Uuid_ItInterface : DbMigration
    {
        public override void Up()
        {
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Patch_Uuid_Interface.sql"));
            AddColumn("dbo.ItInterface", "Created", c => c.DateTime(precision: 7, storeType: "datetime2"));
            CreateIndex("dbo.ItInterface", "Uuid", unique: true, name: "UX_ItInterface_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItInterface", "UX_ItInterface_Uuid");
            DropColumn("dbo.ItInterface", "Created");
        }
    }
}
