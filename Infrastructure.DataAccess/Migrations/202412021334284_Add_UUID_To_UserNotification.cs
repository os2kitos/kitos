using System.Data.Entity.Migrations;

namespace Infrastructure.DataAccess.Migrations
{
    public partial class Add_UUID_To_UserNotification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserNotifications", "Uuid", c => c.Guid(nullable: false));
            CreateIndex("dbo.UserNotifications", "Uuid", unique: true);
        }

        public override void Down()
        {
            DropIndex("dbo.UserNotifications", "Uuid");
            DropColumn("dbo.UserNotifications", "Uuid");
        }

    }
}
