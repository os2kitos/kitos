namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class added_uuid_to_external_reference : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.ExternalReferences", new[] { "Itcontract_Id" });
            DropIndex("dbo.ExternalReferences", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ExternalReferences", new[] { "LastChangedByUserId" });
            AddColumn("dbo.ExternalReferences", "Uuid", c => c.Guid(nullable: false));
            AlterColumn("dbo.ExternalReferences", "ObjectOwnerId", c => c.Int(nullable: false));
            AlterColumn("dbo.ExternalReferences", "LastChangedByUserId", c => c.Int(nullable: false));
            CreateIndex("dbo.ExternalReferences", "Uuid", unique: true, name: "UX_ExternalReference_Uuid");
            CreateIndex("dbo.ExternalReferences", "ItContract_Id");
            CreateIndex("dbo.ExternalReferences", "ObjectOwnerId");
            CreateIndex("dbo.ExternalReferences", "LastChangedByUserId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ExternalReferences", new[] { "LastChangedByUserId" });
            DropIndex("dbo.ExternalReferences", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ExternalReferences", new[] { "ItContract_Id" });
            DropIndex("dbo.ExternalReferences", "UX_ExternalReference_Uuid");
            AlterColumn("dbo.ExternalReferences", "LastChangedByUserId", c => c.Int());
            AlterColumn("dbo.ExternalReferences", "ObjectOwnerId", c => c.Int());
            DropColumn("dbo.ExternalReferences", "Uuid");
            CreateIndex("dbo.ExternalReferences", "LastChangedByUserId");
            CreateIndex("dbo.ExternalReferences", "ObjectOwnerId");
            CreateIndex("dbo.ExternalReferences", "Itcontract_Id");
        }
    }
}
