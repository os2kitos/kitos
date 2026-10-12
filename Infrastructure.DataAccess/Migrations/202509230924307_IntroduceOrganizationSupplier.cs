namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IntroduceOrganizationSupplier : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.OrganizationSuppliers",
                c => new
                    {
                        SupplierId = c.Int(nullable: false),
                        OrganizationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.SupplierId, t.OrganizationId })
                .ForeignKey("dbo.Organization", t => t.OrganizationId, cascadeDelete: true)
                .ForeignKey("dbo.Organization", t => t.SupplierId)
                .Index(t => t.SupplierId)
                .Index(t => t.OrganizationId);
            
            AddColumn("dbo.Organization", "IsSupplier", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.OrganizationSuppliers", "SupplierId", "dbo.Organization");
            DropForeignKey("dbo.OrganizationSuppliers", "OrganizationId", "dbo.Organization");
            DropIndex("dbo.OrganizationSuppliers", new[] { "OrganizationId" });
            DropIndex("dbo.OrganizationSuppliers", new[] { "SupplierId" });
            DropColumn("dbo.Organization", "IsSupplier");
            DropTable("dbo.OrganizationSuppliers");
        }
    }
}
