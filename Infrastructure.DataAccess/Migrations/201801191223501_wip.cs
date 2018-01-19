namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class wip : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.DataProtectionAdvisors", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataProtectionAdvisors", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataResponsibles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataResponsibles", new[] { "LastChangedByUserId" });
            AlterColumn("dbo.DataProtectionAdvisors", "Cvr", c => c.String(maxLength: 10));
            AlterColumn("dbo.DataProtectionAdvisors", "ObjectOwnerId", c => c.Int(nullable: false));
            AlterColumn("dbo.DataProtectionAdvisors", "LastChangedByUserId", c => c.Int(nullable: false));
            AlterColumn("dbo.DataResponsibles", "Cvr", c => c.String(maxLength: 10));
            AlterColumn("dbo.DataResponsibles", "ObjectOwnerId", c => c.Int(nullable: false));
            AlterColumn("dbo.DataResponsibles", "LastChangedByUserId", c => c.Int(nullable: false));
            CreateIndex("dbo.DataProtectionAdvisors", "Cvr");
            CreateIndex("dbo.DataProtectionAdvisors", "ObjectOwnerId");
            CreateIndex("dbo.DataProtectionAdvisors", "LastChangedByUserId");
            CreateIndex("dbo.DataResponsibles", "Cvr");
            CreateIndex("dbo.DataResponsibles", "ObjectOwnerId");
            CreateIndex("dbo.DataResponsibles", "LastChangedByUserId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.DataResponsibles", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataResponsibles", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataResponsibles", new[] { "Cvr" });
            DropIndex("dbo.DataProtectionAdvisors", new[] { "LastChangedByUserId" });
            DropIndex("dbo.DataProtectionAdvisors", new[] { "ObjectOwnerId" });
            DropIndex("dbo.DataProtectionAdvisors", new[] { "Cvr" });
            AlterColumn("dbo.DataResponsibles", "LastChangedByUserId", c => c.Int());
            AlterColumn("dbo.DataResponsibles", "ObjectOwnerId", c => c.Int());
            AlterColumn("dbo.DataResponsibles", "Cvr", c => c.Int(nullable: false));
            AlterColumn("dbo.DataProtectionAdvisors", "LastChangedByUserId", c => c.Int());
            AlterColumn("dbo.DataProtectionAdvisors", "ObjectOwnerId", c => c.Int());
            AlterColumn("dbo.DataProtectionAdvisors", "Cvr", c => c.Int(nullable: false));
            CreateIndex("dbo.DataResponsibles", "LastChangedByUserId");
            CreateIndex("dbo.DataResponsibles", "ObjectOwnerId");
            CreateIndex("dbo.DataProtectionAdvisors", "LastChangedByUserId");
            CreateIndex("dbo.DataProtectionAdvisors", "ObjectOwnerId");
        }
    }
}
