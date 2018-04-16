namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class contactpersonaddedtoorganization : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organization", "ContactPersonId", c => c.Int());
            CreateIndex("dbo.Organization", "ContactPersonId");
            AddForeignKey("dbo.Organization", "ContactPersonId", "dbo.User", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Organization", "ContactPersonId", "dbo.User");
            DropIndex("dbo.Organization", new[] { "ContactPersonId" });
            DropColumn("dbo.Organization", "ContactPersonId");
        }
    }
}
