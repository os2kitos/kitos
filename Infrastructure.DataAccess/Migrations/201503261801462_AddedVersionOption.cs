namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedVersionOption : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "VersionOptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        IsActive = c.Boolean(nullable: false),
                        IsSuggestion = c.Boolean(nullable: false),
                        Note = c.String(unicode: false),
                        ObjectOwnerId = c.Int(),
                        LastChanged = c.DateTime(nullable: false, precision: 0),
                        LastChangedByUserId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("ItInterface", "VersionOptionId", c => c.Int());
            CreateIndex("ItInterface", "VersionOptionId");
            AddForeignKey("ItInterface", "VersionOptionId", "VersionOptions", "Id");
            DropColumn("ItInterface", "Version");
        }
        
        public override void Down()
        {
            AddColumn("ItInterface", "Version", c => c.String(unicode: false));
            DropForeignKey("ItInterface", "VersionOptionId", "VersionOptions");
            DropForeignKey("VersionOptions", "FK_dbo.VersionOptions_dbo.User_ObjectOwnerId");
            DropForeignKey("VersionOptions", "FK_dbo.VersionOptions_dbo.User_LastChangedByUserId");
            DropIndex("VersionOptions", new[] { "LastChangedByUserId" });
            DropIndex("VersionOptions", new[] { "ObjectOwnerId" });
            DropIndex("ItInterface", new[] { "VersionOptionId" });
            DropColumn("ItInterface", "VersionOptionId");
            DropTable("VersionOptions");
        }
    }
}
