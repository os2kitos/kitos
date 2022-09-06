namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixArchiveSupplier : DbMigration
    {
        public override void Up()
        {
            Sql(@"UPDATE ItSystemUsage 
                  SET SupplierId = NULL 
                  WHERE SupplierId not in (SELECT Id from Organization);"
            );

            RenameColumn("dbo.ItSystemUsage", "SupplierId", "ArchiveSupplierId");
            CreateIndex("dbo.ItSystemUsage", "ArchiveSupplierId");
            AddForeignKey("dbo.ItSystemUsage", "ArchiveSupplierId", "dbo.Organization", "Id");
            DropColumn("dbo.ItSystemUsage", "ArchiveSupplier");
        }
        
        public override void Down()
        {
            RenameColumn("dbo.ItSystemUsage", "ArchiveSupplierId", "SupplierId");
            AddColumn("dbo.ItSystemUsage", "ArchiveSupplier", c => c.String());
            DropForeignKey("dbo.ItSystemUsage", "ArchiveSupplierId", "dbo.Organization");
            DropIndex("dbo.ItSystemUsage", new[] { "ArchiveSupplierId" });
        }
    }
}
