namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamedIsDisabledtoIsEnabledonoptionEntity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContractRoles", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.BusinessTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.FrequencyTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ArchiveTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.AgreementElementTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTemplateTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.GoalTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectRoles", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.OrganizationUnitRoles", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.HandoverTrialTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.OptionExtendTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentFreqencyTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentModelTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.PriceRegulationTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProcurementStrategyTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.PurchaseFormTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.TerminationDeadlineTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemRoles", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.SensitiveDataTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.InterfaceTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItInterfaceTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.MethodTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.TsaTypes", "IsEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ReportCategoryTypes", "IsEnabled", c => c.Boolean(nullable: false));
            DropColumn("dbo.ItContractRoles", "IsDisabled");
            DropColumn("dbo.ItSystemTypes", "IsDisabled");
            DropColumn("dbo.BusinessTypes", "IsDisabled");
            DropColumn("dbo.DataTypes", "IsDisabled");
            DropColumn("dbo.FrequencyTypes", "IsDisabled");
            DropColumn("dbo.ArchiveTypes", "IsDisabled");
            DropColumn("dbo.AgreementElementTypes", "IsDisabled");
            DropColumn("dbo.ItContractTemplateTypes", "IsDisabled");
            DropColumn("dbo.ItContractTypes", "IsDisabled");
            DropColumn("dbo.GoalTypes", "IsDisabled");
            DropColumn("dbo.ItProjectTypes", "IsDisabled");
            DropColumn("dbo.ItProjectRoles", "IsDisabled");
            DropColumn("dbo.OrganizationUnitRoles", "IsDisabled");
            DropColumn("dbo.HandoverTrialTypes", "IsDisabled");
            DropColumn("dbo.OptionExtendTypes", "IsDisabled");
            DropColumn("dbo.PaymentFreqencyTypes", "IsDisabled");
            DropColumn("dbo.PaymentModelTypes", "IsDisabled");
            DropColumn("dbo.PriceRegulationTypes", "IsDisabled");
            DropColumn("dbo.ProcurementStrategyTypes", "IsDisabled");
            DropColumn("dbo.PurchaseFormTypes", "IsDisabled");
            DropColumn("dbo.TerminationDeadlineTypes", "IsDisabled");
            DropColumn("dbo.ItSystemRoles", "IsDisabled");
            DropColumn("dbo.SensitiveDataTypes", "IsDisabled");
            DropColumn("dbo.InterfaceTypes", "IsDisabled");
            DropColumn("dbo.ItInterfaceTypes", "IsDisabled");
            DropColumn("dbo.MethodTypes", "IsDisabled");
            DropColumn("dbo.TsaTypes", "IsDisabled");
            DropColumn("dbo.ReportCategoryTypes", "IsDisabled");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ReportCategoryTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.TsaTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.MethodTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItInterfaceTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.InterfaceTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.SensitiveDataTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemRoles", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.TerminationDeadlineTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.PurchaseFormTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProcurementStrategyTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.PriceRegulationTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentModelTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentFreqencyTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.OptionExtendTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.HandoverTrialTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.OrganizationUnitRoles", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectRoles", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.GoalTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTemplateTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.AgreementElementTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ArchiveTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.FrequencyTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.BusinessTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemTypes", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractRoles", "IsDisabled", c => c.Boolean(nullable: false));
            DropColumn("dbo.ReportCategoryTypes", "IsEnabled");
            DropColumn("dbo.TsaTypes", "IsEnabled");
            DropColumn("dbo.MethodTypes", "IsEnabled");
            DropColumn("dbo.ItInterfaceTypes", "IsEnabled");
            DropColumn("dbo.InterfaceTypes", "IsEnabled");
            DropColumn("dbo.SensitiveDataTypes", "IsEnabled");
            DropColumn("dbo.ItSystemRoles", "IsEnabled");
            DropColumn("dbo.TerminationDeadlineTypes", "IsEnabled");
            DropColumn("dbo.PurchaseFormTypes", "IsEnabled");
            DropColumn("dbo.ProcurementStrategyTypes", "IsEnabled");
            DropColumn("dbo.PriceRegulationTypes", "IsEnabled");
            DropColumn("dbo.PaymentModelTypes", "IsEnabled");
            DropColumn("dbo.PaymentFreqencyTypes", "IsEnabled");
            DropColumn("dbo.OptionExtendTypes", "IsEnabled");
            DropColumn("dbo.HandoverTrialTypes", "IsEnabled");
            DropColumn("dbo.OrganizationUnitRoles", "IsEnabled");
            DropColumn("dbo.ItProjectRoles", "IsEnabled");
            DropColumn("dbo.ItProjectTypes", "IsEnabled");
            DropColumn("dbo.GoalTypes", "IsEnabled");
            DropColumn("dbo.ItContractTypes", "IsEnabled");
            DropColumn("dbo.ItContractTemplateTypes", "IsEnabled");
            DropColumn("dbo.AgreementElementTypes", "IsEnabled");
            DropColumn("dbo.ArchiveTypes", "IsEnabled");
            DropColumn("dbo.FrequencyTypes", "IsEnabled");
            DropColumn("dbo.DataTypes", "IsEnabled");
            DropColumn("dbo.BusinessTypes", "IsEnabled");
            DropColumn("dbo.ItSystemTypes", "IsEnabled");
            DropColumn("dbo.ItContractRoles", "IsEnabled");
        }
    }
}
