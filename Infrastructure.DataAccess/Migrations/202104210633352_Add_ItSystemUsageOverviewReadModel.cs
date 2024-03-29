﻿namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_ItSystemUsageOverviewReadModel : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.TaskRef", "UX_TaskKey");
            CreateTable(
                "dbo.ItSystemUsageOverviewReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrganizationId = c.Int(nullable: false),
                        SourceEntityId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        ItSystemDisabled = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        ParentItSystemName = c.String(maxLength: 100),
                        ParentItSystemId = c.Int(),
                        ParentItSystemDisabled = c.Boolean(),
                        Version = c.String(maxLength: 100),
                        LocalCallName = c.String(maxLength: 100),
                        LocalSystemId = c.String(maxLength: 200),
                        ItSystemUuid = c.String(maxLength: 50),
                        ResponsibleOrganizationUnitId = c.Int(),
                        ResponsibleOrganizationUnitName = c.String(maxLength: 100),
                        ItSystemBusinessTypeId = c.Int(),
                        ItSystemBusinessTypeName = c.String(maxLength: 150),
                        ItSystemRightsHolderId = c.Int(),
                        ItSystemRightsHolderName = c.String(maxLength: 100),
                        ItSystemKLEIdsAsCsv = c.String(),
                        ItSystemKLENamesAsCsv = c.String(),
                        LocalReferenceDocumentId = c.String(),
                        LocalReferenceUrl = c.String(),
                        LocalReferenceTitle = c.String(maxLength: 100),
                        ObjectOwnerId = c.Int(),
                        ObjectOwnerName = c.String(maxLength: 100),
                        LastChangedById = c.Int(),
                        LastChangedByName = c.String(maxLength: 100),
                        LastChanged = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        Concluded = c.DateTime(precision: 7, storeType: "datetime2"),
                        MainContractId = c.Int(),
                        MainContractSupplierId = c.Int(),
                        MainContractSupplierName = c.String(maxLength: 100),
                        MainContractIsActive = c.Boolean(),
                        HasMainContract = c.Boolean(nullable: false),
                        SensitiveDataLevelsAsCsv = c.String(),
                        ItProjectNamesAsCsv = c.String(),
                        ArchiveDuty = c.Int(),
                        IsHoldingDocument = c.Boolean(nullable: false),
                        RiskSupervisionDocumentationName = c.String(maxLength: 150),
                        RiskSupervisionDocumentationUrl = c.String(),
                        LinkToDirectoryName = c.String(maxLength: 150),
                        LinkToDirectoryUrl = c.String(),
                        DataProcessingRegistrationsConcludedAsCsv = c.String(),
                        DataProcessingRegistrationNamesAsCsv = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .ForeignKey("dbo.ItSystemUsage", t => t.SourceEntityId)
                .Index(t => t.OrganizationId)
                .Index(t => t.SourceEntityId)
                .Index(t => t.Name, name: "ItSystemUsageOverviewReadModel_Index_Name")
                .Index(t => t.ItSystemDisabled, name: "ItSystemUsageOverviewReadModel_Index_ItSystemDisabled")
                .Index(t => t.ParentItSystemName, name: "ItSystemUsageOverviewReadModel_Index_ItSystemParentName")
                .Index(t => t.Version, name: "ItSystemUsageOverviewReadModel_Index_Version")
                .Index(t => t.LocalCallName, name: "ItSystemUsageOverviewReadModel_Index_LocalCallName")
                .Index(t => t.LocalSystemId, name: "ItSystemUsageOverviewReadModel_Index_LocalSystemId")
                .Index(t => t.ItSystemUuid, name: "ItSystemUsageOverviewReadModel_Index_ItSystemUuid")
                .Index(t => t.ResponsibleOrganizationUnitId, name: "ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationId")
                .Index(t => t.ResponsibleOrganizationUnitName, name: "ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationName")
                .Index(t => t.ItSystemBusinessTypeId, name: "ItSystemUsageOverviewReadModel_Index_ItSystemBusinessTypeId")
                .Index(t => t.ItSystemBusinessTypeName, name: "ItSystemUsageOverviewReadModel_Index_ItSystemBusinessTypeName")
                .Index(t => t.ItSystemRightsHolderId, name: "ItSystemUsageOverviewReadModel_Index_ItSystemBelongsToId")
                .Index(t => t.ItSystemRightsHolderName, name: "ItSystemUsageOverviewReadModel_Index_ItSystemBelongsToName")
                .Index(t => t.LocalReferenceTitle, name: "ItSystemUsageOverviewReadModel_Index_LocalReferenceTitle")
                .Index(t => t.ObjectOwnerId, name: "ItSystemUsageOverviewReadModel_Index_ObjectOwnerId")
                .Index(t => t.ObjectOwnerName, name: "ItSystemUsageOverviewReadModel_Index_ObjectOwnerName")
                .Index(t => t.LastChangedById, name: "ItSystemUsageOverviewReadModel_Index_LastChangedById")
                .Index(t => t.LastChangedByName, name: "ItSystemUsageOverviewReadModel_Index_LastChangedByName")
                .Index(t => t.MainContractId, name: "ItSystemUsageOverviewReadModel_Index_MainContractId")
                .Index(t => t.MainContractSupplierId, name: "ItSystemUsageOverviewReadModel_Index_MainContractSupplierId")
                .Index(t => t.MainContractSupplierName, name: "ItSystemUsageOverviewReadModel_Index_MainContractSupplierName")
                .Index(t => t.HasMainContract, name: "ItSystemUsageOverviewReadModel_Index_HasMainContract")
                .Index(t => t.ArchiveDuty, name: "ItSystemUsageOverviewReadModel_Index_ArchiveDuty")
                .Index(t => t.IsHoldingDocument, name: "ItSystemUsageOverviewReadModel_Index_IsHoldingDocument")
                .Index(t => t.RiskSupervisionDocumentationName, name: "ItSystemUsage_Index_RiskSupervisionDocumentationName")
                .Index(t => t.LinkToDirectoryName, name: "ItSystemUsage_Index_LinkToDirectoryName");
            
            CreateTable(
                "dbo.ItSystemUsageOverviewArchivePeriodReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        EndDate = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsageOverviewReadModels", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.StartDate, name: "ItSystemUsageOverviewArchivePeriodReadModel_index_StartDate")
                .Index(t => t.EndDate, name: "ItSystemUsageOverviewArchivePeriodReadModel_index_EndDate")
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.ItSystemUsageOverviewDataProcessingRegistrationReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DataProcessingRegistrationId = c.Int(nullable: false),
                        DataProcessingRegistrationName = c.String(nullable: false, maxLength: 200),
                        IsAgreementConcluded = c.Int(),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsageOverviewReadModels", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.DataProcessingRegistrationId, name: "ItSystemUsageOverviewArchivePeriodReadModel_index_DataProcessingRegistrationId")
                .Index(t => t.DataProcessingRegistrationName, name: "ItSystemUsageOverviewArchivePeriodReadModel_index_DataProcessingRegistrationName")
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.ItSystemUsageOverviewItProjectReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItProjectId = c.Int(nullable: false),
                        ItProjectName = c.String(nullable: false, maxLength: 150),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsageOverviewReadModels", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.ItProjectId, name: "ItSystemUsageOverviewItProjectReadModel_index_ItProjectId")
                .Index(t => t.ItProjectName, name: "ItSystemUsageOverviewItProjectReadModel_index_ItProjectName")
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.ItSystemUsageOverviewTaskRefReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        KLEId = c.String(maxLength: 15),
                        KLEName = c.String(maxLength: 150),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsageOverviewReadModels", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.KLEId, name: "ItSystemUsageOverviewTaskRefReadModel_Index_KLEId")
                .Index(t => t.KLEName, name: "ItSystemUsageOverviewTaskRefReadModel_Index_KLEName")
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.ItSystemUsageOverviewRoleAssignmentReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RoleId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        UserFullName = c.String(nullable: false, maxLength: 100),
                        Email = c.String(maxLength: 100),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsageOverviewReadModels", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.RoleId)
                .Index(t => t.UserId)
                .Index(t => t.UserFullName)
                .Index(t => t.Email)
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.ItSystemUsageOverviewSensitiveDataLevelReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SensitivityDataLevel = c.Int(nullable: false),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItSystemUsageOverviewReadModels", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.SensitivityDataLevel, name: "ItSystemUsageOverviewSensitiveDataLevelReadModel_Index_SensitiveDataLevel")
                .Index(t => t.ParentId);
            
            AlterColumn("dbo.ItSystemUsage", "LocalSystemId", c => c.String(maxLength: 200));
            AlterColumn("dbo.ItSystemUsage", "Version", c => c.String(maxLength: 100));
            AlterColumn("dbo.ItSystemUsage", "LocalCallName", c => c.String(maxLength: 100));
            AlterColumn("dbo.ItSystemUsage", "LinkToDirectoryUrlName", c => c.String(maxLength: 150));
            AlterColumn("dbo.ItSystemUsage", "RiskSupervisionDocumentationUrlName", c => c.String(maxLength: 150));
            AlterColumn("dbo.AgreementElementTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.GoalTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.TaskRef", "TaskKey", c => c.String(maxLength: 15));
            AlterColumn("dbo.TaskRef", "Description", c => c.String(maxLength: 150));
            AlterColumn("dbo.OrganizationUnitRoles", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.ItProjectTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.ItProjectRoles", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.DataTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.InterfaceTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.RelationFrequencyTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.ItContractTemplateTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.ItContractTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.HandoverTrialTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.OptionExtendTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.PaymentFreqencyTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.PaymentModelTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.PriceRegulationTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.ProcurementStrategyTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.PurchaseFormTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.ItContractRoles", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.TerminationDeadlineTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.DataProcessingBasisForTransferOptions", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.DataProcessingDataResponsibleOptions", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.DataProcessingCountryOptions", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.DataProcessingOversightOptions", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.DataProcessingRegistrationRoles", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.ItSystemRoles", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.ArchiveTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.SensitiveDataTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            AlterColumn("dbo.BusinessTypes", "Name", c => c.String(nullable: false, maxLength: 150));
            CreateIndex("dbo.ItSystemUsage", "LocalSystemId", name: "ItSystemUsage_Index_LocalSystemId");
            CreateIndex("dbo.ItSystemUsage", "Version", name: "ItSystemUsage_Index_Version");
            CreateIndex("dbo.ItSystemUsage", "LocalCallName", name: "ItSystemUsage_Index_LocalCallName");
            CreateIndex("dbo.ItSystemUsage", "LinkToDirectoryUrlName", name: "ItSystemUsage_Index_LinkToDirectoryUrlName");
            CreateIndex("dbo.ItSystemUsage", "RiskSupervisionDocumentationUrlName", name: "ItSystemUsage_Index_RiskSupervisionDocumentationUrlName");
            CreateIndex("dbo.TaskRef", "TaskKey", unique: true, name: "UX_TaskKey");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItSystemUsageOverviewReadModels", "SourceEntityId", "dbo.ItSystemUsage");
            DropForeignKey("dbo.ItSystemUsageOverviewSensitiveDataLevelReadModels", "ParentId", "dbo.ItSystemUsageOverviewReadModels");
            DropForeignKey("dbo.ItSystemUsageOverviewRoleAssignmentReadModels", "ParentId", "dbo.ItSystemUsageOverviewReadModels");
            DropForeignKey("dbo.ItSystemUsageOverviewReadModels", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItSystemUsageOverviewTaskRefReadModels", "ParentId", "dbo.ItSystemUsageOverviewReadModels");
            DropForeignKey("dbo.ItSystemUsageOverviewItProjectReadModels", "ParentId", "dbo.ItSystemUsageOverviewReadModels");
            DropForeignKey("dbo.ItSystemUsageOverviewDataProcessingRegistrationReadModels", "ParentId", "dbo.ItSystemUsageOverviewReadModels");
            DropForeignKey("dbo.ItSystemUsageOverviewArchivePeriodReadModels", "ParentId", "dbo.ItSystemUsageOverviewReadModels");
            DropIndex("dbo.TaskRef", "UX_TaskKey");
            DropIndex("dbo.ItSystemUsageOverviewSensitiveDataLevelReadModels", new[] { "ParentId" });
            DropIndex("dbo.ItSystemUsageOverviewSensitiveDataLevelReadModels", "ItSystemUsageOverviewSensitiveDataLevelReadModel_Index_SensitiveDataLevel");
            DropIndex("dbo.ItSystemUsageOverviewRoleAssignmentReadModels", new[] { "ParentId" });
            DropIndex("dbo.ItSystemUsageOverviewRoleAssignmentReadModels", new[] { "Email" });
            DropIndex("dbo.ItSystemUsageOverviewRoleAssignmentReadModels", new[] { "UserFullName" });
            DropIndex("dbo.ItSystemUsageOverviewRoleAssignmentReadModels", new[] { "UserId" });
            DropIndex("dbo.ItSystemUsageOverviewRoleAssignmentReadModels", new[] { "RoleId" });
            DropIndex("dbo.ItSystemUsageOverviewTaskRefReadModels", new[] { "ParentId" });
            DropIndex("dbo.ItSystemUsageOverviewTaskRefReadModels", "ItSystemUsageOverviewTaskRefReadModel_Index_KLEName");
            DropIndex("dbo.ItSystemUsageOverviewTaskRefReadModels", "ItSystemUsageOverviewTaskRefReadModel_Index_KLEId");
            DropIndex("dbo.ItSystemUsageOverviewItProjectReadModels", new[] { "ParentId" });
            DropIndex("dbo.ItSystemUsageOverviewItProjectReadModels", "ItSystemUsageOverviewItProjectReadModel_index_ItProjectName");
            DropIndex("dbo.ItSystemUsageOverviewItProjectReadModels", "ItSystemUsageOverviewItProjectReadModel_index_ItProjectId");
            DropIndex("dbo.ItSystemUsageOverviewDataProcessingRegistrationReadModels", new[] { "ParentId" });
            DropIndex("dbo.ItSystemUsageOverviewDataProcessingRegistrationReadModels", "ItSystemUsageOverviewArchivePeriodReadModel_index_DataProcessingRegistrationName");
            DropIndex("dbo.ItSystemUsageOverviewDataProcessingRegistrationReadModels", "ItSystemUsageOverviewArchivePeriodReadModel_index_DataProcessingRegistrationId");
            DropIndex("dbo.ItSystemUsageOverviewArchivePeriodReadModels", new[] { "ParentId" });
            DropIndex("dbo.ItSystemUsageOverviewArchivePeriodReadModels", "ItSystemUsageOverviewArchivePeriodReadModel_index_EndDate");
            DropIndex("dbo.ItSystemUsageOverviewArchivePeriodReadModels", "ItSystemUsageOverviewArchivePeriodReadModel_index_StartDate");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsage_Index_LinkToDirectoryName");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsage_Index_RiskSupervisionDocumentationName");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_IsHoldingDocument");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ArchiveDuty");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_HasMainContract");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_MainContractSupplierName");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_MainContractSupplierId");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_MainContractId");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_LastChangedByName");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_LastChangedById");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ObjectOwnerName");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ObjectOwnerId");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_LocalReferenceTitle");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemBelongsToName");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemBelongsToId");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemBusinessTypeName");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemBusinessTypeId");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationName");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ResponsibleOrganizationId");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemUuid");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_LocalSystemId");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_LocalCallName");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_Version");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemParentName");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_ItSystemDisabled");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", "ItSystemUsageOverviewReadModel_Index_Name");
            DropIndex("dbo.ItSystemUsageOverviewReadModels", new[] { "SourceEntityId" });
            DropIndex("dbo.ItSystemUsageOverviewReadModels", new[] { "OrganizationId" });
            DropIndex("dbo.ItSystemUsage", "ItSystemUsage_Index_RiskSupervisionDocumentationUrlName");
            DropIndex("dbo.ItSystemUsage", "ItSystemUsage_Index_LinkToDirectoryUrlName");
            DropIndex("dbo.ItSystemUsage", "ItSystemUsage_Index_LocalCallName");
            DropIndex("dbo.ItSystemUsage", "ItSystemUsage_Index_Version");
            DropIndex("dbo.ItSystemUsage", "ItSystemUsage_Index_LocalSystemId");
            AlterColumn("dbo.BusinessTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.SensitiveDataTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.ArchiveTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.ItSystemRoles", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.DataProcessingRegistrationRoles", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.DataProcessingOversightOptions", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.DataProcessingCountryOptions", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.DataProcessingDataResponsibleOptions", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.DataProcessingBasisForTransferOptions", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.TerminationDeadlineTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.ItContractRoles", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.PurchaseFormTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.ProcurementStrategyTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.PriceRegulationTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.PaymentModelTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.PaymentFreqencyTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.OptionExtendTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.HandoverTrialTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.ItContractTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.ItContractTemplateTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.RelationFrequencyTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.InterfaceTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.DataTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.ItProjectRoles", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.ItProjectTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.OrganizationUnitRoles", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.TaskRef", "Description", c => c.String());
            AlterColumn("dbo.TaskRef", "TaskKey", c => c.String(maxLength: 50));
            AlterColumn("dbo.GoalTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.AgreementElementTypes", "Name", c => c.String(nullable: false));
            AlterColumn("dbo.ItSystemUsage", "RiskSupervisionDocumentationUrlName", c => c.String());
            AlterColumn("dbo.ItSystemUsage", "LinkToDirectoryUrlName", c => c.String());
            AlterColumn("dbo.ItSystemUsage", "LocalCallName", c => c.String());
            AlterColumn("dbo.ItSystemUsage", "Version", c => c.String());
            AlterColumn("dbo.ItSystemUsage", "LocalSystemId", c => c.String());
            DropTable("dbo.ItSystemUsageOverviewSensitiveDataLevelReadModels");
            DropTable("dbo.ItSystemUsageOverviewRoleAssignmentReadModels");
            DropTable("dbo.ItSystemUsageOverviewTaskRefReadModels");
            DropTable("dbo.ItSystemUsageOverviewItProjectReadModels");
            DropTable("dbo.ItSystemUsageOverviewDataProcessingRegistrationReadModels");
            DropTable("dbo.ItSystemUsageOverviewArchivePeriodReadModels");
            DropTable("dbo.ItSystemUsageOverviewReadModels");
            CreateIndex("dbo.TaskRef", "TaskKey", unique: true, name: "UX_TaskKey");
        }
    }
}
