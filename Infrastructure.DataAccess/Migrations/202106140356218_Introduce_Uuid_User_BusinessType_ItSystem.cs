namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Infrastructure.DataAccess.Tools;

    public partial class Introduce_Uuid_User_BusinessType_ItSystem : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystem", "Created", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.User", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.BusinessTypes", "Uuid", c => c.Guid(nullable: false));

            //Make sure uuid defaults are initialized before adding the indexes
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Patch_Uuid_System_User.sql"));

            CreateIndex("dbo.ItSystem", "Uuid", unique: true, name: "UX_System_Uuuid");
            CreateIndex("dbo.User", "Uuid", unique: true, name: "UX_User_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.User", "UX_User_Uuid");
            DropIndex("dbo.ItSystem", "UX_System_Uuuid");
            DropColumn("dbo.BusinessTypes", "Uuid");
            DropColumn("dbo.User", "Uuid");
            DropColumn("dbo.ItSystem", "Created");
        }
    }
}
