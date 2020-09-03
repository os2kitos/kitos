using Infrastructure.DataAccess.Tools;

namespace Infrastructure.DataAccess.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class Local_Admin_Toggle_DataProcessing : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Config", "ShowDataProcessing", c => c.Boolean(nullable: false));

            SqlResource(SqlMigrationScriptRepository.GetResourceName("EnableDataProcessing.sql"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Config", "ShowDataProcessing");
        }
    }
}
