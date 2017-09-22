namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenamedOptionEntityIsActivetoIsLocallyAvailable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContractRoles", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.BusinessTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.FrequencyTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ArchiveTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.AgreementElementTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTemplateTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.GoalTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectRoles", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.OrganizationUnitRoles", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.HandoverTrialTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.OptionExtendTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentFreqencyTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentModelTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.PriceRegulationTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProcurementStrategyTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.PurchaseFormTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.TerminationDeadlineTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemRoles", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.SensitiveDataTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.InterfaceTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItInterfaceTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.MethodTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.TsaTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ReportCategoryTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            DropColumn("dbo.ItContractRoles", "IsActive");
            DropColumn("dbo.ItSystemTypes", "IsActive");
            DropColumn("dbo.BusinessTypes", "IsActive");
            DropColumn("dbo.DataTypes", "IsActive");
            DropColumn("dbo.FrequencyTypes", "IsActive");
            DropColumn("dbo.ArchiveTypes", "IsActive");
            DropColumn("dbo.AgreementElementTypes", "IsActive");
            DropColumn("dbo.ItContractTemplateTypes", "IsActive");
            DropColumn("dbo.ItContractTypes", "IsActive");
            DropColumn("dbo.GoalTypes", "IsActive");
            DropColumn("dbo.ItProjectTypes", "IsActive");
            DropColumn("dbo.ItProjectRoles", "IsActive");
            DropColumn("dbo.OrganizationUnitRoles", "IsActive");
            DropColumn("dbo.HandoverTrialTypes", "IsActive");
            DropColumn("dbo.OptionExtendTypes", "IsActive");
            DropColumn("dbo.PaymentFreqencyTypes", "IsActive");
            DropColumn("dbo.PaymentModelTypes", "IsActive");
            DropColumn("dbo.PriceRegulationTypes", "IsActive");
            DropColumn("dbo.ProcurementStrategyTypes", "IsActive");
            DropColumn("dbo.PurchaseFormTypes", "IsActive");
            DropColumn("dbo.TerminationDeadlineTypes", "IsActive");
            DropColumn("dbo.ItSystemRoles", "IsActive");
            DropColumn("dbo.SensitiveDataTypes", "IsActive");
            DropColumn("dbo.InterfaceTypes", "IsActive");
            DropColumn("dbo.ItInterfaceTypes", "IsActive");
            DropColumn("dbo.MethodTypes", "IsActive");
            DropColumn("dbo.TsaTypes", "IsActive");
            DropColumn("dbo.ReportCategoryTypes", "IsActive");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ReportCategoryTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.TsaTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.MethodTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItInterfaceTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.InterfaceTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.SensitiveDataTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemRoles", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.TerminationDeadlineTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.PurchaseFormTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProcurementStrategyTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.PriceRegulationTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentModelTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentFreqencyTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.OptionExtendTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.HandoverTrialTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.OrganizationUnitRoles", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectRoles", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.GoalTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTemplateTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.AgreementElementTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ArchiveTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.FrequencyTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.BusinessTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractRoles", "IsActive", c => c.Boolean(nullable: false));
            DropColumn("dbo.ReportCategoryTypes", "IsLocallyAvaliable");
            DropColumn("dbo.TsaTypes", "IsLocallyAvaliable");
            DropColumn("dbo.MethodTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ItInterfaceTypes", "IsLocallyAvaliable");
            DropColumn("dbo.InterfaceTypes", "IsLocallyAvaliable");
            DropColumn("dbo.SensitiveDataTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ItSystemRoles", "IsLocallyAvaliable");
            DropColumn("dbo.TerminationDeadlineTypes", "IsLocallyAvaliable");
            DropColumn("dbo.PurchaseFormTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ProcurementStrategyTypes", "IsLocallyAvaliable");
            DropColumn("dbo.PriceRegulationTypes", "IsLocallyAvaliable");
            DropColumn("dbo.PaymentModelTypes", "IsLocallyAvaliable");
            DropColumn("dbo.PaymentFreqencyTypes", "IsLocallyAvaliable");
            DropColumn("dbo.OptionExtendTypes", "IsLocallyAvaliable");
            DropColumn("dbo.HandoverTrialTypes", "IsLocallyAvaliable");
            DropColumn("dbo.OrganizationUnitRoles", "IsLocallyAvaliable");
            DropColumn("dbo.ItProjectRoles", "IsLocallyAvaliable");
            DropColumn("dbo.ItProjectTypes", "IsLocallyAvaliable");
            DropColumn("dbo.GoalTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ItContractTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ItContractTemplateTypes", "IsLocallyAvaliable");
            DropColumn("dbo.AgreementElementTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ArchiveTypes", "IsLocallyAvaliable");
            DropColumn("dbo.FrequencyTypes", "IsLocallyAvaliable");
            DropColumn("dbo.DataTypes", "IsLocallyAvaliable");
            DropColumn("dbo.BusinessTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ItSystemTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ItContractRoles", "IsLocallyAvaliable");
        }
    }
}
