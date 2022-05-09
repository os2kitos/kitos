namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_References_From_Organization : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ExternalReferences", "Organization_Id", "dbo.Organization");
            DropIndex("dbo.ExternalReferences", new[] { "Organization_Id" });
            DropColumn("dbo.ExternalReferences", "Organization_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ExternalReferences", "Organization_Id", c => c.Int());
            CreateIndex("dbo.ExternalReferences", "Organization_Id");
            AddForeignKey("dbo.ExternalReferences", "Organization_Id", "dbo.Organization", "Id");
        }
    }
}
