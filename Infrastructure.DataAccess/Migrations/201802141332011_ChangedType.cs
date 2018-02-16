namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ItSystemUsage", "ArchiveDuty", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ItSystemUsage", "ArchiveDuty", c => c.Boolean());
        }
    }
}
