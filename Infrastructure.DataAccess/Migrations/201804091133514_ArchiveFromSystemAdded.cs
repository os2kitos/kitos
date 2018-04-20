namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ArchiveFromSystemAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "ArchiveFromSystem", c => c.Boolean());
            AlterColumn("dbo.ItSystemUsage", "ArchiveFreq", c => c.Int());
            AlterColumn("dbo.ItSystemUsage", "Registertype", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ItSystemUsage", "Registertype", c => c.Boolean(nullable: false));
            AlterColumn("dbo.ItSystemUsage", "ArchiveFreq", c => c.Int(nullable: false));
            DropColumn("dbo.ItSystemUsage", "ArchiveFromSystem");
        }
    }
}
