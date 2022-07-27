namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class fixArchiveSupplier : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "ArchiveSupplierId", c => c.Int());
            CreateIndex("dbo.ItSystemUsage", "ArchiveSupplierId");
            AddForeignKey("dbo.ItSystemUsage", "ArchiveSupplierId", "dbo.Organization", "Id");
            DropColumn("dbo.ItSystemUsage", "ArchiveSupplier");
            DropColumn("dbo.ItSystemUsage", "SupplierId");
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
