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
                  WHERE SupplierId IN (SELECT SupplierId 
	                 FROM ItSystemUsage T0 
	                 LEFT JOIN Organization T1
	                 ON T0.SupplierId = T1.Id
	                 WHERE T1.Id IS NULL);"
            );

            RenameColumn("dbo.ItSystemUsage", "SupplierId", "ArchiveSupplierId");
            CreateIndex("dbo.ItSystemUsage", "ArchiveSupplierId");
            AddForeignKey("dbo.ItSystemUsage", "ArchiveSupplierId", "dbo.Organization", "Id");
            DropColumn("dbo.ItSystemUsage", "ArchiveSupplier");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItSystemUsage", "SupplierId", c => c.Int());
            AddColumn("dbo.ItSystemUsage", "ArchiveSupplier", c => c.String());
            DropForeignKey("dbo.ItSystemUsage", "ArchiveSupplierId", "dbo.Organization");
            DropIndex("dbo.ItSystemUsage", new[] { "ArchiveSupplierId" });
            DropColumn("dbo.ItSystemUsage", "ArchiveSupplierId");
        }
    }
}
