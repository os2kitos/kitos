namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using Infrastructure.DataAccess.Tools;
    using System.Data.Entity.Migrations;
    
    public partial class ITSystemUsage_Remove_DirectoryOrUrlRef : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ItSystemUsage", "DirectoryOrUrlRef");
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Remove_Orphan_AttachedOptions.sql"));
        }

        public override void Down()
        {
            AddColumn("dbo.ItSystemUsage", "DirectoryOrUrlRef", c => c.String());
        }
    }
}
