namespace Infrastructure.DataAccess.Migrations
{
    using Infrastructure.DataAccess.Tools;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Added_Uuid_To_OptionTypes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ArchiveLocations", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.AgreementElementTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.GoalTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.OrganizationUnitRoles", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ReportCategoryTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ItProjectTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ItProjectRoles", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.DataTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.InterfaceTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.RelationFrequencyTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ItContractTemplateTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ItContractTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.HandoverTrialTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.OptionExtendTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.PaymentFreqencyTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.PaymentModelTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.PriceRegulationTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ProcurementStrategyTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.PurchaseFormTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ItContractRoles", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.TerminationDeadlineTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.DataProcessingBasisForTransferOptions", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.DataProcessingDataResponsibleOptions", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.DataProcessingCountryOptions", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.DataProcessingOversightOptions", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.DataProcessingRegistrationRoles", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ItSystemRoles", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ArchiveTestLocations", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ArchiveTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.ItSystemCategories", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.SensitiveDataTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.TerminationDeadlineTypesInSystems", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.BusinessTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.RegisterTypes", "Uuid", c => c.Guid(nullable: false));
            AddColumn("dbo.SensitivePersonalDataTypes", "Uuid", c => c.Guid(nullable: false));

            SqlResource(SqlMigrationScriptRepository.GetResourceName("Migrate_Uuid_On_OptionTypes.sql"));

            CreateIndex("dbo.AgreementElementTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.GoalTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.OrganizationUnitRoles", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.ItProjectTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.ItProjectRoles", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.DataTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.InterfaceTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.RelationFrequencyTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.ItContractTemplateTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.ItContractTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.HandoverTrialTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.OptionExtendTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.PaymentFreqencyTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.PaymentModelTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.PriceRegulationTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.ProcurementStrategyTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.PurchaseFormTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.ItContractRoles", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.TerminationDeadlineTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.DataProcessingBasisForTransferOptions", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.DataProcessingDataResponsibleOptions", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.DataProcessingCountryOptions", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.DataProcessingOversightOptions", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.DataProcessingRegistrationRoles", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.ItSystemRoles", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.ArchiveTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.SensitiveDataTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
            CreateIndex("dbo.BusinessTypes", "Uuid", unique: true, name: "UX_Option_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.BusinessTypes", "UX_Option_Uuid");
            DropIndex("dbo.SensitiveDataTypes", "UX_Option_Uuid");
            DropIndex("dbo.ArchiveTypes", "UX_Option_Uuid");
            DropIndex("dbo.ItSystemRoles", "UX_Option_Uuid");
            DropIndex("dbo.DataProcessingRegistrationRoles", "UX_Option_Uuid");
            DropIndex("dbo.DataProcessingOversightOptions", "UX_Option_Uuid");
            DropIndex("dbo.DataProcessingCountryOptions", "UX_Option_Uuid");
            DropIndex("dbo.DataProcessingDataResponsibleOptions", "UX_Option_Uuid");
            DropIndex("dbo.DataProcessingBasisForTransferOptions", "UX_Option_Uuid");
            DropIndex("dbo.TerminationDeadlineTypes", "UX_Option_Uuid");
            DropIndex("dbo.ItContractRoles", "UX_Option_Uuid");
            DropIndex("dbo.PurchaseFormTypes", "UX_Option_Uuid");
            DropIndex("dbo.ProcurementStrategyTypes", "UX_Option_Uuid");
            DropIndex("dbo.PriceRegulationTypes", "UX_Option_Uuid");
            DropIndex("dbo.PaymentModelTypes", "UX_Option_Uuid");
            DropIndex("dbo.PaymentFreqencyTypes", "UX_Option_Uuid");
            DropIndex("dbo.OptionExtendTypes", "UX_Option_Uuid");
            DropIndex("dbo.HandoverTrialTypes", "UX_Option_Uuid");
            DropIndex("dbo.ItContractTypes", "UX_Option_Uuid");
            DropIndex("dbo.ItContractTemplateTypes", "UX_Option_Uuid");
            DropIndex("dbo.RelationFrequencyTypes", "UX_Option_Uuid");
            DropIndex("dbo.InterfaceTypes", "UX_Option_Uuid");
            DropIndex("dbo.DataTypes", "UX_Option_Uuid");
            DropIndex("dbo.ItProjectRoles", "UX_Option_Uuid");
            DropIndex("dbo.ItProjectTypes", "UX_Option_Uuid");
            DropIndex("dbo.OrganizationUnitRoles", "UX_Option_Uuid");
            DropIndex("dbo.GoalTypes", "UX_Option_Uuid");
            DropIndex("dbo.AgreementElementTypes", "UX_Option_Uuid");
            DropColumn("dbo.SensitivePersonalDataTypes", "Uuid");
            DropColumn("dbo.RegisterTypes", "Uuid");
            DropColumn("dbo.BusinessTypes", "Uuid");
            DropColumn("dbo.TerminationDeadlineTypesInSystems", "Uuid");
            DropColumn("dbo.SensitiveDataTypes", "Uuid");
            DropColumn("dbo.ItSystemCategories", "Uuid");
            DropColumn("dbo.ArchiveTypes", "Uuid");
            DropColumn("dbo.ArchiveTestLocations", "Uuid");
            DropColumn("dbo.ItSystemRoles", "Uuid");
            DropColumn("dbo.DataProcessingRegistrationRoles", "Uuid");
            DropColumn("dbo.DataProcessingOversightOptions", "Uuid");
            DropColumn("dbo.DataProcessingCountryOptions", "Uuid");
            DropColumn("dbo.DataProcessingDataResponsibleOptions", "Uuid");
            DropColumn("dbo.DataProcessingBasisForTransferOptions", "Uuid");
            DropColumn("dbo.TerminationDeadlineTypes", "Uuid");
            DropColumn("dbo.ItContractRoles", "Uuid");
            DropColumn("dbo.PurchaseFormTypes", "Uuid");
            DropColumn("dbo.ProcurementStrategyTypes", "Uuid");
            DropColumn("dbo.PriceRegulationTypes", "Uuid");
            DropColumn("dbo.PaymentModelTypes", "Uuid");
            DropColumn("dbo.PaymentFreqencyTypes", "Uuid");
            DropColumn("dbo.OptionExtendTypes", "Uuid");
            DropColumn("dbo.HandoverTrialTypes", "Uuid");
            DropColumn("dbo.ItContractTypes", "Uuid");
            DropColumn("dbo.ItContractTemplateTypes", "Uuid");
            DropColumn("dbo.RelationFrequencyTypes", "Uuid");
            DropColumn("dbo.InterfaceTypes", "Uuid");
            DropColumn("dbo.DataTypes", "Uuid");
            DropColumn("dbo.ItProjectRoles", "Uuid");
            DropColumn("dbo.ItProjectTypes", "Uuid");
            DropColumn("dbo.ReportCategoryTypes", "Uuid");
            DropColumn("dbo.OrganizationUnitRoles", "Uuid");
            DropColumn("dbo.GoalTypes", "Uuid");
            DropColumn("dbo.AgreementElementTypes", "Uuid");
            DropColumn("dbo.ArchiveLocations", "Uuid");
        }
    }
}
