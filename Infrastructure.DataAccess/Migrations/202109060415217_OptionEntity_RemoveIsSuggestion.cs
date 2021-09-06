namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OptionEntity_RemoveIsSuggestion : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AgreementElementTypes", "IsSuggestion");
            DropColumn("dbo.ArchiveLocations", "IsSuggestion");
            DropColumn("dbo.ArchiveTestLocations", "IsSuggestion");
            DropColumn("dbo.ArchiveTypes", "IsSuggestion");
            DropColumn("dbo.DataTypes", "IsSuggestion");
            DropColumn("dbo.InterfaceTypes", "IsSuggestion");
            DropColumn("dbo.GoalTypes", "IsSuggestion");
            DropColumn("dbo.ItProjectTypes", "IsSuggestion");
            DropColumn("dbo.OrganizationUnitRoles", "IsSuggestion");
            DropColumn("dbo.ItProjectRoles", "IsSuggestion");
            DropColumn("dbo.ItSystemCategories", "IsSuggestion");
            DropColumn("dbo.ItSystemRoles", "IsSuggestion");
            DropColumn("dbo.SensitiveDataTypes", "IsSuggestion");
            DropColumn("dbo.RelationFrequencyTypes", "IsSuggestion");
            DropColumn("dbo.ItContractTemplateTypes", "IsSuggestion");
            DropColumn("dbo.ItContractTypes", "IsSuggestion");
            DropColumn("dbo.HandoverTrialTypes", "IsSuggestion");
            DropColumn("dbo.OptionExtendTypes", "IsSuggestion");
            DropColumn("dbo.PaymentFreqencyTypes", "IsSuggestion");
            DropColumn("dbo.PaymentModelTypes", "IsSuggestion");
            DropColumn("dbo.PriceRegulationTypes", "IsSuggestion");
            DropColumn("dbo.ProcurementStrategyTypes", "IsSuggestion");
            DropColumn("dbo.PurchaseFormTypes", "IsSuggestion");
            DropColumn("dbo.ItContractRoles", "IsSuggestion");
            DropColumn("dbo.TerminationDeadlineTypes", "IsSuggestion");
            DropColumn("dbo.DataProcessingBasisForTransferOptions", "IsSuggestion");
            DropColumn("dbo.DataProcessingDataResponsibleOptions", "IsSuggestion");
            DropColumn("dbo.DataProcessingCountryOptions", "IsSuggestion");
            DropColumn("dbo.DataProcessingOversightOptions", "IsSuggestion");
            DropColumn("dbo.DataProcessingRegistrationRoles", "IsSuggestion");
            DropColumn("dbo.ReportCategoryTypes", "IsSuggestion");
            DropColumn("dbo.BusinessTypes", "IsSuggestion");
            DropColumn("dbo.RegisterTypes", "IsSuggestion");
            DropColumn("dbo.SensitivePersonalDataTypes", "IsSuggestion");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SensitivePersonalDataTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.RegisterTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.BusinessTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ReportCategoryTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataProcessingRegistrationRoles", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataProcessingOversightOptions", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataProcessingCountryOptions", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataProcessingDataResponsibleOptions", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataProcessingBasisForTransferOptions", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.TerminationDeadlineTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractRoles", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.PurchaseFormTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProcurementStrategyTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.PriceRegulationTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentModelTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentFreqencyTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.OptionExtendTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.HandoverTrialTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTemplateTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.RelationFrequencyTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.SensitiveDataTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemRoles", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemCategories", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectRoles", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.OrganizationUnitRoles", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.GoalTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.InterfaceTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ArchiveTypes", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ArchiveTestLocations", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.ArchiveLocations", "IsSuggestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.AgreementElementTypes", "IsSuggestion", c => c.Boolean(nullable: false));
        }
    }
}
