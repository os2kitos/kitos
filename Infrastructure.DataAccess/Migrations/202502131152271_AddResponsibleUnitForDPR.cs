namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddResponsibleUnitForDPR : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DataProcessingRegistrations", "ResponsibleOrganizationUnitId", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "ResponsibleOrgUnitUuid", c => c.Guid());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "ResponsibleOrgUnitId", c => c.Int());
            AddColumn("dbo.DataProcessingRegistrationReadModels", "ResponsibleOrgUnitName", c => c.String());
            CreateIndex("dbo.DataProcessingRegistrations", "ResponsibleOrganizationUnitId");
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "ResponsibleOrgUnitUuid", name: "IX_DPR_ResponsibleOrgUnitUuid");
            CreateIndex("dbo.DataProcessingRegistrationReadModels", "ResponsibleOrgUnitId", name: "IX_DPR_ResponsibleOrgUnitId");
            AddForeignKey("dbo.DataProcessingRegistrations", "ResponsibleOrganizationUnitId", "dbo.OrganizationUnit", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.DataProcessingRegistrations", "ResponsibleOrganizationUnitId", "dbo.OrganizationUnit");
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DPR_ResponsibleOrgUnitId");
            DropIndex("dbo.DataProcessingRegistrationReadModels", "IX_DPR_ResponsibleOrgUnitUuid");
            DropIndex("dbo.DataProcessingRegistrations", new[] { "ResponsibleOrganizationUnitId" });
            DropColumn("dbo.DataProcessingRegistrationReadModels", "ResponsibleOrgUnitName");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "ResponsibleOrgUnitId");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "ResponsibleOrgUnitUuid");
            DropColumn("dbo.DataProcessingRegistrations", "ResponsibleOrganizationUnitId");
        }
    }
}
