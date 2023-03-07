namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
using Infrastructure.DataAccess.Tools;

    public partial class Add_Uuid_To_DataRow : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataRow", "Uuid", c => c.Guid(nullable: false));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Patch_Uuid_DataRow.sql"));
            CreateIndex("dbo.DataRow", "Uuid", unique: true, name: "UX_uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DataRow", "UX_uuid");
            DropColumn("dbo.DataRow", "Uuid");
        }
    }
}
