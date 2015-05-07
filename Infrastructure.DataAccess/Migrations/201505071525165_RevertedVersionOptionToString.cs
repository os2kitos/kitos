namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RevertedVersionOptionToString : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("VersionOptions", "LastChangedByUserId", "User");
            DropForeignKey("VersionOptions", "ObjectOwnerId", "User");
            DropForeignKey("ItInterface", "VersionOptionId", "VersionOptions");
            DropIndex("ItInterface", new[] { "VersionOptionId" });
            DropIndex("VersionOptions", new[] { "ObjectOwnerId" });
            DropIndex("VersionOptions", new[] { "LastChangedByUserId" });
            AddColumn("ItInterface", "Version", c => c.String(maxLength: 20, storeType: "nvarchar"));
            DropColumn("ItInterface", "VersionOptionId");
            DropTable("VersionOptions");
        }
        
        public override void Down()
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
            DropColumn("ItInterface", "Version");
            CreateIndex("VersionOptions", "LastChangedByUserId");
            CreateIndex("VersionOptions", "ObjectOwnerId");
            CreateIndex("ItInterface", "VersionOptionId");
            AddForeignKey("ItInterface", "VersionOptionId", "VersionOptions", "Id");
            AddForeignKey("VersionOptions", "ObjectOwnerId", "User", "Id");
            AddForeignKey("VersionOptions", "LastChangedByUserId", "User", "Id");
        }
    }
}
