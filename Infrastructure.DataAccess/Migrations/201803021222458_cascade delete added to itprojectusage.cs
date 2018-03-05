namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class cascadedeleteaddedtoitprojectusage : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ItProjectOrgUnitUsages", "OrganizationUnitId", "dbo.OrganizationUnit");
            AddForeignKey("dbo.ItProjectOrgUnitUsages", "OrganizationUnitId", "dbo.OrganizationUnit", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItProjectOrgUnitUsages", "OrganizationUnitId", "dbo.OrganizationUnit");
            AddForeignKey("dbo.ItProjectOrgUnitUsages", "OrganizationUnitId", "dbo.OrganizationUnit", "Id");
        }
    }
}
