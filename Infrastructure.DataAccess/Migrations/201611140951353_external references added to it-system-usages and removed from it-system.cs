namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class externalreferencesaddedtoitsystemusagesandremovedfromitsystem : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ExternalReferences", "ItSystem_Id", "dbo.ItSystem");
            DropIndex("dbo.ExternalReferences", new[] { "ItSystem_Id" });
            AddColumn("dbo.ExternalReferences", "ItSystemUsage_Id", c => c.Int());
            CreateIndex("dbo.ExternalReferences", "ItSystemUsage_Id");
            AddForeignKey("dbo.ExternalReferences", "ItSystemUsage_Id", "dbo.ItSystemUsage", "Id", cascadeDelete: true);
            DropColumn("dbo.ExternalReferences", "ItSystem_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ExternalReferences", "ItSystem_Id", c => c.Int());
            DropForeignKey("dbo.ExternalReferences", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropIndex("dbo.ExternalReferences", new[] { "ItSystemUsage_Id" });
            DropColumn("dbo.ExternalReferences", "ItSystemUsage_Id");
            CreateIndex("dbo.ExternalReferences", "ItSystem_Id");
            AddForeignKey("dbo.ExternalReferences", "ItSystem_Id", "dbo.ItSystem", "Id", cascadeDelete: true);
        }
    }
}
