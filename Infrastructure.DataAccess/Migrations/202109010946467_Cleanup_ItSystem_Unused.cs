namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Cleanup_ItSystem_Unused : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ArchivePeriod", "ItSystem_Id", "dbo.ItSystem");
            DropIndex("dbo.ArchivePeriod", new[] { "ItSystem_Id" });
            DropColumn("dbo.ItSystem", "LinkToDirectoryAdminUrl");
            DropColumn("dbo.ItSystem", "LinkToDirectoryAdminUrlName");
            DropColumn("dbo.ArchivePeriod", "ItSystem_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ArchivePeriod", "ItSystem_Id", c => c.Int());
            AddColumn("dbo.ItSystem", "LinkToDirectoryAdminUrlName", c => c.String());
            AddColumn("dbo.ItSystem", "LinkToDirectoryAdminUrl", c => c.String());
            CreateIndex("dbo.ArchivePeriod", "ItSystem_Id");
            AddForeignKey("dbo.ArchivePeriod", "ItSystem_Id", "dbo.ItSystem", "Id");
        }
    }
}
