using System.Data.Entity.Migrations;
using Infrastructure.DataAccess.Tools;

namespace Infrastructure.DataAccess.Migrations
{
    public partial class Add_UUID_To_UserNotification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserNotifications", "Uuid", c => c.Guid(nullable: false));
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Patch_Uuid_UserNotification.sql"));
            CreateIndex("dbo.UserNotifications", "Uuid", unique: true);
        }

        public override void Down()
        {
            DropIndex("dbo.UserNotifications",  "Uuid");
            DropColumn("dbo.UserNotifications", "Uuid");
        }
    }
}