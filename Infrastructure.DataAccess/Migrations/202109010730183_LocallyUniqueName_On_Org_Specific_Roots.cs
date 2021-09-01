namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LocallyUniqueName_On_Org_Specific_Roots : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.DataProcessingRegistrations", new[] { "OrganizationId" });
            DropIndex("dbo.ItContract", new[] { "OrganizationId" });
            DropIndex("dbo.ItProject", new[] { "OrganizationId" });
            RenameIndex(table: "dbo.ItSystem", name: "UX_NamePerOrg", newName: "UX_NameUniqueToOrg");
            RenameIndex(table: "dbo.DataProcessingRegistrations", name: "DataProcessingRegistration_Index_Name", newName: "IX_Name");
            RenameIndex(table: "dbo.ItContract", name: "Contract_Index_Name", newName: "IX_Name");
            RenameIndex(table: "dbo.ItInterface", name: "UX_NamePerOrg", newName: "UX_NameAndVersionUniqueToOrg");
            RenameIndex(table: "dbo.ItProject", name: "Project_Index_Name", newName: "IX_Name");
            CreateIndex("dbo.ItSystem", "Name");
            CreateIndex("dbo.ItSystem", "OrganizationId");
            CreateIndex("dbo.DataProcessingRegistrations", new[] { "OrganizationId", "Name" }, unique: true, name: "UX_NameUniqueToOrg");
            CreateIndex("dbo.DataProcessingRegistrations", "OrganizationId");
            CreateIndex("dbo.ItContract", new[] { "OrganizationId", "Name" }, unique: true, name: "UX_NameUniqueToOrg");
            CreateIndex("dbo.ItContract", "OrganizationId");
            CreateIndex("dbo.ItInterface", "Version");
            CreateIndex("dbo.ItInterface", "Name");
            CreateIndex("dbo.ItInterface", "OrganizationId");
            CreateIndex("dbo.ItProject", new[] { "OrganizationId", "Name" }, unique: true, name: "UX_NameUniqueToOrg");
            CreateIndex("dbo.ItProject", "OrganizationId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItProject", new[] { "OrganizationId" });
            DropIndex("dbo.ItProject", "UX_NameUniqueToOrg");
            DropIndex("dbo.ItInterface", new[] { "OrganizationId" });
            DropIndex("dbo.ItInterface", new[] { "Name" });
            DropIndex("dbo.ItInterface", new[] { "Version" });
            DropIndex("dbo.ItContract", new[] { "OrganizationId" });
            DropIndex("dbo.ItContract", "UX_NameUniqueToOrg");
            DropIndex("dbo.DataProcessingRegistrations", new[] { "OrganizationId" });
            DropIndex("dbo.DataProcessingRegistrations", "UX_NameUniqueToOrg");
            DropIndex("dbo.ItSystem", new[] { "OrganizationId" });
            DropIndex("dbo.ItSystem", new[] { "Name" });
            RenameIndex(table: "dbo.ItProject", name: "IX_Name", newName: "Project_Index_Name");
            RenameIndex(table: "dbo.ItInterface", name: "UX_NameAndVersionUniqueToOrg", newName: "UX_NamePerOrg");
            RenameIndex(table: "dbo.ItContract", name: "IX_Name", newName: "Contract_Index_Name");
            RenameIndex(table: "dbo.DataProcessingRegistrations", name: "IX_Name", newName: "DataProcessingRegistration_Index_Name");
            RenameIndex(table: "dbo.ItSystem", name: "UX_NameUniqueToOrg", newName: "UX_NamePerOrg");
            CreateIndex("dbo.ItProject", "OrganizationId");
            CreateIndex("dbo.ItContract", "OrganizationId");
            CreateIndex("dbo.DataProcessingRegistrations", "OrganizationId");
        }
    }
}
