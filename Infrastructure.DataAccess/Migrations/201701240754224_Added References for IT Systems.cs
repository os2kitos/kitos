namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedReferencesforITSystems : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystem", "ReferenceId", c => c.Int());
            AddColumn("dbo.ExternalReferences", "ItSystem_Id", c => c.Int());
            CreateIndex("dbo.ItSystem", "ReferenceId");
            CreateIndex("dbo.ExternalReferences", "ItSystem_Id");
            AddForeignKey("dbo.ExternalReferences", "ItSystem_Id", "dbo.ItSystem", "Id");
            AddForeignKey("dbo.ItSystem", "ReferenceId", "dbo.ExternalReferences", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystem", "ReferenceId", "dbo.ExternalReferences");
            DropForeignKey("dbo.ExternalReferences", "ItSystem_Id", "dbo.ItSystem");
            DropIndex("dbo.ExternalReferences", new[] { "ItSystem_Id" });
            DropIndex("dbo.ItSystem", new[] { "ReferenceId" });
            DropColumn("dbo.ExternalReferences", "ItSystem_Id");
            DropColumn("dbo.ItSystem", "ReferenceId");
        }
    }
}
