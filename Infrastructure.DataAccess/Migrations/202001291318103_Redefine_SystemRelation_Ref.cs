namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Redefine_SystemRelation_Ref : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ExternalLinks", "LastChangedByUserId", "dbo.User");
            DropForeignKey("dbo.ExternalLinks", "ObjectOwnerId", "dbo.User");
            DropForeignKey("dbo.SystemRelations", "Reference_Id", "dbo.ExternalLinks");
            DropIndex("dbo.SystemRelations", new[] { "Reference_Id" });
            DropIndex("dbo.ExternalLinks", new[] { "ObjectOwnerId" });
            DropIndex("dbo.ExternalLinks", new[] { "LastChangedByUserId" });
            AddColumn("dbo.SystemRelations", "Reference", c => c.String());
            DropColumn("dbo.SystemRelations", "Reference_Id");
            DropTable("dbo.ExternalLinks");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ExternalLinks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Url = c.String(),
                        ObjectOwnerId = c.Int(nullable: false),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LastChangedByUserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.SystemRelations", "Reference_Id", c => c.Int());
            DropColumn("dbo.SystemRelations", "Reference");
            CreateIndex("dbo.ExternalLinks", "LastChangedByUserId");
            CreateIndex("dbo.ExternalLinks", "ObjectOwnerId");
            CreateIndex("dbo.SystemRelations", "Reference_Id");
            AddForeignKey("dbo.SystemRelations", "Reference_Id", "dbo.ExternalLinks", "Id");
            AddForeignKey("dbo.ExternalLinks", "ObjectOwnerId", "dbo.User", "Id");
            AddForeignKey("dbo.ExternalLinks", "LastChangedByUserId", "dbo.User", "Id");
        }
    }
}
