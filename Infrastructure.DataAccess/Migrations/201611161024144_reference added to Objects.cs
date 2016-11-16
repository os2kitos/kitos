namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class referenceaddedtoObjects : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemUsage", "ReferenceId", c => c.Int());
            AddColumn("dbo.ItContract", "ReferenceId", c => c.Int());
            AddColumn("dbo.ItProject", "ReferenceId", c => c.Int());
            CreateIndex("dbo.ItSystemUsage", "ReferenceId");
            CreateIndex("dbo.ItContract", "ReferenceId");
            CreateIndex("dbo.ItProject", "ReferenceId");
            AddForeignKey("dbo.ItProject", "ReferenceId", "dbo.ExternalReferences", "Id");
            AddForeignKey("dbo.ItContract", "ReferenceId", "dbo.ExternalReferences", "Id");
            AddForeignKey("dbo.ItSystemUsage", "ReferenceId", "dbo.ExternalReferences", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystemUsage", "ReferenceId", "dbo.ExternalReferences");
            DropForeignKey("dbo.ItContract", "ReferenceId", "dbo.ExternalReferences");
            DropForeignKey("dbo.ItProject", "ReferenceId", "dbo.ExternalReferences");
            DropIndex("dbo.ItProject", new[] { "ReferenceId" });
            DropIndex("dbo.ItContract", new[] { "ReferenceId" });
            DropIndex("dbo.ItSystemUsage", new[] { "ReferenceId" });
            DropColumn("dbo.ItProject", "ReferenceId");
            DropColumn("dbo.ItContract", "ReferenceId");
            DropColumn("dbo.ItSystemUsage", "ReferenceId");
        }
    }
}
