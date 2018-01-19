namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class referencetoorganizationadded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProtectionAdvisors", "Organization_Id", c => c.Int());
            CreateIndex("dbo.DataProtectionAdvisors", "Organization_Id");
            AddForeignKey("dbo.DataProtectionAdvisors", "Organization_Id", "dbo.Organization", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProtectionAdvisors", "Organization_Id", "dbo.Organization");
            DropIndex("dbo.DataProtectionAdvisors", new[] { "Organization_Id" });
            DropColumn("dbo.DataProtectionAdvisors", "Organization_Id");
        }
    }
}
