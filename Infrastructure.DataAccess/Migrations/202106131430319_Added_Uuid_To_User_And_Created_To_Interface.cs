namespace Infrastructure.DataAccess.Migrations
{
    using Infrastructure.DataAccess.Tools;
    using System.Data.Entity.Migrations;

    public partial class Added_Uuid_To_User_And_Created_To_Interface : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystem", "Created", c => c.DateTime(precision: 7, storeType: "datetime2"));
            AddColumn("dbo.User", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ItInterface", "Created", c => c.DateTime(precision: 7, storeType: "datetime2"));

            SqlResource(SqlMigrationScriptRepository.GetResourceName("AddUuidToUsers.sql"));

            CreateIndex("dbo.User", "Uuid", unique: true, name: "UX_User_Uuid");

        }
        
        public override void Down()
        {
            DropIndex("dbo.User", "UX_User_Uuid");
            DropColumn("dbo.ItInterface", "Created");
            DropColumn("dbo.User", "Uuid");
            DropColumn("dbo.ItSystem", "Created");
        }
    }
}
