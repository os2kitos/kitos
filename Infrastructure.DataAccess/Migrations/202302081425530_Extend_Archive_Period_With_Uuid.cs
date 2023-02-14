namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Extend_Archive_Period_With_Uuid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ArchivePeriod", "Uuid", c => c.Guid(nullable: false));
            CreateIndex("dbo.ArchivePeriod", "Uuid", name: "UX_ArchivePeriod_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ArchivePeriod", "UX_ArchivePeriod_Uuid");
            DropColumn("dbo.ArchivePeriod", "Uuid");
        }
    }
}
