namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using Infrastructure.DataAccess.Tools;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateOrganizationUnit : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TaskRef", "OrganizationUnit_Id", "dbo.OrganizationUnit");
            DropForeignKey("dbo.ItSystemUsageOrganizationUnits", "ItSystemUsage_Id", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsageOrganizationUnits", "OrganizationUnit_Id", "dbo.OrganizationUnit");
            DropIndex("dbo.TaskRef", new[] { "OrganizationUnit_Id" });
            DropIndex("dbo.ItSystemUsageOrganizationUnits", new[] { "ItSystemUsage_Id" });
            DropIndex("dbo.ItSystemUsageOrganizationUnits", new[] { "OrganizationUnit_Id" });
            AddColumn("dbo.OrganizationUnit", "Uuid", c => c.Guid(nullable: false));

            //Make sure all uuids are unique before index is added
            SqlResource(SqlMigrationScriptRepository.GetResourceName("Patch_Uuid_OrganizationUnit.sql"));

            CreateIndex("dbo.OrganizationUnit", "Uuid", unique: true, name: "UX_OrganizationUnit_UUID");
            DropColumn("dbo.TaskRef", "OrganizationUnit_Id");
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
            
            AddColumn("dbo.TaskRef", "OrganizationUnit_Id", c => c.Int());
            DropIndex("dbo.OrganizationUnit", "UX_OrganizationUnit_UUID");
            DropColumn("dbo.OrganizationUnit", "Uuid");
            CreateIndex("dbo.ItSystemUsageOrganizationUnits", "OrganizationUnit_Id");
            CreateIndex("dbo.ItSystemUsageOrganizationUnits", "ItSystemUsage_Id");
            CreateIndex("dbo.TaskRef", "OrganizationUnit_Id");
            AddForeignKey("dbo.ItSystemUsageOrganizationUnits", "OrganizationUnit_Id", "dbo.OrganizationUnit", "Id");
            AddForeignKey("dbo.ItSystemUsageOrganizationUnits", "ItSystemUsage_Id", "dbo.ItSystemUsage", "Id");
            AddForeignKey("dbo.TaskRef", "OrganizationUnit_Id", "dbo.OrganizationUnit", "Id");
        }
    }
}
