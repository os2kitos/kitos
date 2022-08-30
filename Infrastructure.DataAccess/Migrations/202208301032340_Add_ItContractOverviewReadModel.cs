namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_ItContractOverviewReadModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ItContractOverviewReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OrganizationId = c.Int(nullable: false),
                        SourceEntityId = c.Int(nullable: false),
                        Name = c.String(maxLength: 200),
                        IsActive = c.Boolean(nullable: false),
                        ContractId = c.String(),
                        ParentContractName = c.String(maxLength: 200),
                        CriticalityId = c.Int(),
                        CriticalityName = c.String(maxLength: 150),
                        ResponsibleOrgUnitId = c.Int(),
                        ResponsibleOrgUnitName = c.String(),
                        SupplierName = c.String(maxLength: 100),
                        ContractSigner = c.String(),
                        ContractTypeId = c.Int(),
                        ContractTypeName = c.String(maxLength: 150),
                        ContractTemplateId = c.Int(),
                        ContractTemplateName = c.String(maxLength: 150),
                        PurchaseFormId = c.Int(),
                        PurchaseFormName = c.String(maxLength: 150),
                        ProcurementStrategyId = c.Int(),
                        ProcurementStrategyName = c.String(maxLength: 150),
                        ProcurementPlanYear = c.Int(),
                        ProcurementPlanQuarter = c.Int(),
                        ProcurementInitiated = c.Int(),
                        DataProcessingAgreementsCsv = c.String(),
                        ItSystemUsagesCsv = c.String(),
                        ItSystemUsagesSystemUuidCsv = c.String(),
                        NumberOfAssociatedSystemRelations = c.Int(nullable: false),
                        ActiveReferenceTitle = c.String(),
                        ActiveReferenceUrl = c.String(),
                        ActiveReferenceExternalReferenceId = c.String(),
                        AccumulatedAcquisitionCost = c.Int(),
                        AccumulatedOperationCost = c.Int(),
                        AccumulatedOtherCost = c.Int(),
                        OperationRemunerationBegunDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        PaymentModelId = c.Int(),
                        PaymentModelName = c.String(maxLength: 150),
                        PaymentFrequencyId = c.Int(),
                        PaymentFrequencyName = c.String(maxLength: 150),
                        LatestAuditDate = c.DateTime(precision: 7, storeType: "datetime2"),
                        AuditStatusWhite = c.Int(),
                        AuditStatusRed = c.Int(),
                        AuditStatusYellow = c.Int(),
                        AuditStatusGreen = c.Int(),
                        AuditStatusMax = c.Int(),
                        Duration = c.String(maxLength: 100),
                        OptionExtendId = c.Int(),
                        OptionExtendName = c.String(maxLength: 150),
                        TerminationDeadlineId = c.Int(),
                        TerminationDeadlineName = c.String(maxLength: 150),
                        IrrevocableTo = c.DateTime(precision: 7, storeType: "datetime2"),
                        TerminatedAt = c.DateTime(precision: 7, storeType: "datetime2"),
                        LastEditedByUserName = c.String(),
                        LastEditedAtDate = c.DateTime(precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Organization", t => t.OrganizationId)
                .ForeignKey("dbo.ItContract", t => t.SourceEntityId, cascadeDelete: true)
                .Index(t => t.OrganizationId)
                .Index(t => t.SourceEntityId)
                .Index(t => t.Name, name: "IX_Contract_Name")
                .Index(t => t.IsActive, name: "IX_Contract_Active")
                .Index(t => t.ParentContractName, name: "IX_ParentContract_Name")
                .Index(t => t.CriticalityId, name: "IX_CriticalityType_Id")
                .Index(t => t.CriticalityName, name: "IX_CriticalityType_Name")
                .Index(t => t.ResponsibleOrgUnitId)
                .Index(t => t.SupplierName)
                .Index(t => t.ContractTypeId, name: "IX_ItContractType_Id")
                .Index(t => t.ContractTypeName, name: "IX_ItContractType_Name")
                .Index(t => t.ContractTemplateId, name: "IX_ItContractTemplateType_Id")
                .Index(t => t.ContractTemplateName, name: "IX_ItContractTemplateType_Name")
                .Index(t => t.PurchaseFormId, name: "IX_PurchaseFormType_Id")
                .Index(t => t.PurchaseFormName, name: "IX_PurchaseFormType_Name")
                .Index(t => t.ProcurementStrategyId, name: "IX_ProcurementStrategyType_Id")
                .Index(t => t.ProcurementStrategyName, name: "IX_ProcurementStrategyType_Name")
                .Index(t => t.ProcurementPlanYear)
                .Index(t => t.ProcurementPlanQuarter)
                .Index(t => t.ProcurementInitiated)
                .Index(t => t.NumberOfAssociatedSystemRelations)
                .Index(t => t.AccumulatedAcquisitionCost)
                .Index(t => t.AccumulatedOperationCost)
                .Index(t => t.AccumulatedOtherCost)
                .Index(t => t.OperationRemunerationBegunDate)
                .Index(t => t.PaymentModelId, name: "IX_PaymentModelType_Id")
                .Index(t => t.PaymentModelName, name: "IX_PaymentModelType_Name")
                .Index(t => t.PaymentFrequencyId, name: "IX_PaymentFreqencyType_Id")
                .Index(t => t.PaymentFrequencyName, name: "IX_PaymentFreqencyType_Name")
                .Index(t => t.LatestAuditDate)
                .Index(t => t.AuditStatusWhite)
                .Index(t => t.AuditStatusRed)
                .Index(t => t.AuditStatusYellow)
                .Index(t => t.AuditStatusGreen)
                .Index(t => t.AuditStatusMax)
                .Index(t => t.Duration)
                .Index(t => t.OptionExtendId, name: "IX_OptionExtendType_Id")
                .Index(t => t.OptionExtendName, name: "IX_OptionExtendType_Name")
                .Index(t => t.TerminationDeadlineId, name: "IX_TerminationDeadlineType_Id")
                .Index(t => t.TerminationDeadlineName, name: "IX_TerminationDeadlineType_Name")
                .Index(t => t.IrrevocableTo)
                .Index(t => t.TerminatedAt)
                .Index(t => t.LastEditedAtDate);
            
            CreateTable(
                "dbo.ItContractOverviewReadModelDataProcessingAgreements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DataProcessingRegistrationId = c.Int(nullable: false),
                        DataProcessingRegistrationName = c.String(maxLength: 200),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContractOverviewReadModels", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.DataProcessingRegistrationName, name: "IX_ItContract_Read_Dpr_Name")
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.ItContractOverviewReadModelItSystemUsages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ItSystemUsageId = c.Int(nullable: false),
                        ItSystemUsageSystemUuid = c.Guid(nullable: false),
                        ItSystemUsageName = c.String(maxLength: 200),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContractOverviewReadModels", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.ItSystemUsageSystemUuid, name: "IX_ItContract_Read_System_Uuid")
                .Index(t => t.ItSystemUsageName, name: "IX_ItContract_Read_System_Name")
                .Index(t => t.ParentId);
            
            CreateTable(
                "dbo.ItContractOverviewRoleAssignmentReadModels",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RoleId = c.Int(nullable: false),
                        UserId = c.Int(nullable: false),
                        UserFullName = c.String(),
                        Email = c.String(),
                        ParentId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ItContractOverviewReadModels", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.Id, name: "IX_ItContract_Read_Role_Id")
                .Index(t => t.UserId, name: "IX_ItContract_Read_User_Id")
                .Index(t => t.ParentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ItContractOverviewReadModels", "SourceEntityId", "dbo.ItContract");
            DropForeignKey("dbo.ItContractOverviewRoleAssignmentReadModels", "ParentId", "dbo.ItContractOverviewReadModels");
            DropForeignKey("dbo.ItContractOverviewReadModels", "OrganizationId", "dbo.Organization");
            DropForeignKey("dbo.ItContractOverviewReadModelItSystemUsages", "ParentId", "dbo.ItContractOverviewReadModels");
            DropForeignKey("dbo.ItContractOverviewReadModelDataProcessingAgreements", "ParentId", "dbo.ItContractOverviewReadModels");
            DropIndex("dbo.ItContractOverviewRoleAssignmentReadModels", new[] { "ParentId" });
            DropIndex("dbo.ItContractOverviewRoleAssignmentReadModels", "IX_ItContract_Read_User_Id");
            DropIndex("dbo.ItContractOverviewRoleAssignmentReadModels", "IX_ItContract_Read_Role_Id");
            DropIndex("dbo.ItContractOverviewReadModelItSystemUsages", new[] { "ParentId" });
            DropIndex("dbo.ItContractOverviewReadModelItSystemUsages", "IX_ItContract_Read_System_Name");
            DropIndex("dbo.ItContractOverviewReadModelItSystemUsages", "IX_ItContract_Read_System_Uuid");
            DropIndex("dbo.ItContractOverviewReadModelDataProcessingAgreements", new[] { "ParentId" });
            DropIndex("dbo.ItContractOverviewReadModelDataProcessingAgreements", "IX_ItContract_Read_Dpr_Name");
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "LastEditedAtDate" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "TerminatedAt" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "IrrevocableTo" });
            DropIndex("dbo.ItContractOverviewReadModels", "IX_TerminationDeadlineType_Name");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_TerminationDeadlineType_Id");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_OptionExtendType_Name");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_OptionExtendType_Id");
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "Duration" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "AuditStatusMax" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "AuditStatusGreen" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "AuditStatusYellow" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "AuditStatusRed" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "AuditStatusWhite" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "LatestAuditDate" });
            DropIndex("dbo.ItContractOverviewReadModels", "IX_PaymentFreqencyType_Name");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_PaymentFreqencyType_Id");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_PaymentModelType_Name");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_PaymentModelType_Id");
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "OperationRemunerationBegunDate" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "AccumulatedOtherCost" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "AccumulatedOperationCost" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "AccumulatedAcquisitionCost" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "NumberOfAssociatedSystemRelations" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "ProcurementInitiated" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "ProcurementPlanQuarter" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "ProcurementPlanYear" });
            DropIndex("dbo.ItContractOverviewReadModels", "IX_ProcurementStrategyType_Name");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_ProcurementStrategyType_Id");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_PurchaseFormType_Name");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_PurchaseFormType_Id");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_ItContractTemplateType_Name");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_ItContractTemplateType_Id");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_ItContractType_Name");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_ItContractType_Id");
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "SupplierName" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "ResponsibleOrgUnitId" });
            DropIndex("dbo.ItContractOverviewReadModels", "IX_CriticalityType_Name");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_CriticalityType_Id");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_ParentContract_Name");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_Contract_Active");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_Contract_Name");
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "SourceEntityId" });
            DropIndex("dbo.ItContractOverviewReadModels", new[] { "OrganizationId" });
            DropTable("dbo.ItContractOverviewRoleAssignmentReadModels");
            DropTable("dbo.ItContractOverviewReadModelItSystemUsages");
            DropTable("dbo.ItContractOverviewReadModelDataProcessingAgreements");
            DropTable("dbo.ItContractOverviewReadModels");
        }
    }
}
