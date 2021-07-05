namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OrganizatoinUnitRemoveUnusedProperties : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItSystemUsageOrganizationUnits", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsageOrganizationUnits", "OrganizationUnit_Id", "dbo.OrganizationUnit");
            DropIndex("dbo.ItSystemUsageOrganizationUnits", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.ItSystemUsageOrganizationUnits", new[] { "OrganizationUnit_Id" });
            DropTable("dbo.ItSystemUsageOrganizationUnits");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ItSystemUsageOrganizationUnits",
                c => new
                    {
                        ItSystemUsage_Id = c.Int(nullable: false),
                        OrganizationUnit_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ItSystemUsage_Id, t.OrganizationUnit_Id });
            
            CreateIndex("dbo.ItSystemUsageOrganizationUnits", "OrganizationUnit_Id");
            CreateIndex("dbo.ItSystemUsageOrganizationUnits", "ItSystemUsage_Id");
            AddForeignKey("dbo.ItSystemUsageOrganizationUnits", "OrganizationUnit_Id", "dbo.OrganizationUnit", "Id");
            AddForeignKey("dbo.ItSystemUsageOrganizationUnits", "ItSystemUsage_Id", "dbo.ItSystemUsage", "Id");
        }
    }
}
