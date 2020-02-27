namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Archive_Duty_Nullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ItSystem", "ArchiveDuty", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ItSystem", "ArchiveDuty", c => c.Int(nullable: false));
        }
    }
}
