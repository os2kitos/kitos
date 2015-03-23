namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MadeVersionIdNullable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("ItInterface", "VersionId", "Versions");
            DropIndex("ItInterface", new[] { "VersionId" });
            AlterColumn("ItInterface", "VersionId", c => c.Int());
            CreateIndex("ItInterface", "VersionId");
            AddForeignKey("ItInterface", "VersionId", "Versions", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("ItInterface", "VersionId", "Versions");
            DropIndex("ItInterface", new[] { "VersionId" });
            AlterColumn("ItInterface", "VersionId", c => c.Int(nullable: false));
            CreateIndex("ItInterface", "VersionId");
            AddForeignKey("ItInterface", "VersionId", "Versions", "Id", cascadeDelete: true);
        }
    }
}
