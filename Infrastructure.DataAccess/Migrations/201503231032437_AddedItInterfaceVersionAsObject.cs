namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedItInterfaceVersionAsObject : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Versions",
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
            
            AddColumn("ItInterface", "VersionId", c => c.Int(nullable: false));
            CreateIndex("ItInterface", "VersionId");
            AddForeignKey("ItInterface", "VersionId", "Versions", "Id", cascadeDelete: true);
            DropColumn("ItInterface", "Version");
        }
        
        public override void Down()
        {
            AddColumn("ItInterface", "Version", c => c.String(unicode: false));
            DropForeignKey("ItInterface", "VersionId", "Versions");
            DropForeignKey("Versions", "FK_dbo.Versions_dbo.User_ObjectOwnerId");
            DropForeignKey("Versions", "FK_dbo.Versions_dbo.User_LastChangedByUserId");
            DropIndex("Versions", new[] { "LastChangedByUserId" });
            DropIndex("Versions", new[] { "ObjectOwnerId" });
            DropIndex("ItInterface", new[] { "VersionId" });
            DropColumn("ItInterface", "VersionId");
            DropTable("Versions");
        }
    }
}
