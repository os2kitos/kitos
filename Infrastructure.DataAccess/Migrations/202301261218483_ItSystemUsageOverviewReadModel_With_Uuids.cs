namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ItSystemUsageOverviewReadModel_With_Uuids : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContractOverviewReadModels", "SourceEntityUuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "SourceEntityUuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "ParentItSystemUuid", c => c.Guid());
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "ResponsibleOrganizationUnitUuid", c => c.Guid());
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "ItSystemBusinessTypeUuid", c => c.Guid());
            AddColumn("dbo.ItSystemUsageOverviewReadModels", "MainContractUuid", c => c.Guid());
            AddColumn("dbo.ItSystemUsageOverviewDataProcessingRegistrationReadModels", "DataProcessingRegistrationUuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ItSystemUsageOverviewInterfaceReadModels", "InterfaceUuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ItSystemUsageOverviewUsedBySystemUsageReadModels", "ItSystemUsageUuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ItSystemUsageOverviewUsingSystemUsageReadModels", "ItSystemUsageUuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ItSystemUsageOverviewRoleAssignmentReadModels", "RoleUuid", c => c.Guid(nullable: false));
            AddColumn("dbo.DataProcessingRegistrationReadModels", "SourceEntityUuid", c => c.Guid(nullable: false));
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "ResponsibleOrganizationUnitUuid", name: "ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationUuid");
            CreateIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemBusinessTypeUuid", name: "ItSystemUsageOverviewReadModel_Index_ItSystemBusinessTypeUuid");
            CreateIndex("dbo.ItSystemUsageOverviewUsedBySystemUsageReadModels", "ItSystemUsageUuid", name: "ItSystemUsageOverviewItSystemUsageReadModel_index_ItSystemUsageUuid");
            CreateIndex("dbo.ItSystemUsageOverviewUsingSystemUsageReadModels", "ItSystemUsageUuid", name: "ItSystemUsageOverviewUsingSystemUsageReadModel_index_ItSystemUsageUuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItSystemUsageOverviewUsingSystemUsageReadModels", "ItSystemUsageOverviewUsingSystemUsageReadModel_index_ItSystemUsageUuid");
            DropIndex("dbo.ItSystemUsageOverviewUsedBySystemUsageReadModels", "ItSystemUsageOverviewItSystemUsageReadModel_index_ItSystemUsageUuid");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemBusinessTypeUuid");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationUuid");
            DropColumn("dbo.DataProcessingRegistrationReadModels", "SourceEntityUuid");
            DropColumn("dbo.ItSystemUsageOverviewRoleAssignmentReadModels", "RoleUuid");
            DropColumn("dbo.ItSystemUsageOverviewUsingSystemUsageReadModels", "ItSystemUsageUuid");
            DropColumn("dbo.ItSystemUsageOverviewUsedBySystemUsageReadModels", "ItSystemUsageUuid");
            DropColumn("dbo.ItSystemUsageOverviewInterfaceReadModels", "InterfaceUuid");
            DropColumn("dbo.ItSystemUsageOverviewDataProcessingRegistrationReadModels", "DataProcessingRegistrationUuid");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "MainContractUuid");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "ItSystemBusinessTypeUuid");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "ResponsibleOrganizationUnitUuid");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "ParentItSystemUuid");
            DropColumn("dbo.ItSystemUsageOverviewReadModels", "SourceEntityUuid");
            DropColumn("dbo.ItContractOverviewReadModels", "SourceEntityUuid");
        }
    }
}
