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
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ItSystem", "ArchiveDuty", c => c.Int(nullable: false));
            DropColumn("dbo.ItSystem", "ArchiveDutyComment");
        }
    }
}
