using Infrastructure.DataAccess.Tools;

namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Archive_Duty_Changes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystem", "ArchiveDutyComment", c => c.String());
            AlterColumn("dbo.ItSystem", "ArchiveDuty", c => c.Int());
            DropColumn("dbo.ItSystemUsage", "Archived");
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Clear_ArchiveDuty_On_ItSystem.sql"));
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsage", "Archived", c => c.Boolean());
            AlterColumn("dbo.ItSystem", "ArchiveDuty", c => c.Int(nullable: false));
            DropColumn("dbo.ItSystem", "ArchiveDutyComment");
        }
    }
}
