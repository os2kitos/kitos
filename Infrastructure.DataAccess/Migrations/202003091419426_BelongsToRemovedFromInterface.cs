namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BelongsToRemovedFromInterface : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItInterface", "BelongsToId", "dbo.Organization");
            DropIndex("dbo.ItInterface", new[] { "BelongsToId" });
            DropColumn("dbo.ItInterface", "BelongsToId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ItInterface", "BelongsToId", c => c.Int());
            CreateIndex("dbo.ItInterface", "BelongsToId");
            AddForeignKey("dbo.ItInterface", "BelongsToId", "dbo.Organization", "Id");
        }
    }
}
