namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsLocallyAvailablefix : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContractRoles", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.BusinessTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.FrequencyTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ArchiveTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.AgreementElementTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTemplateTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.GoalTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectRoles", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.OrganizationUnitRoles", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.HandoverTrialTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.OptionExtendTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentFreqencyTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentModelTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.PriceRegulationTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProcurementStrategyTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.PurchaseFormTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.TerminationDeadlineTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemRoles", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.SensitiveDataTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.InterfaceTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItInterfaceTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.MethodTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.TsaTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ReportCategoryTypes", "IsLocallyAvailable", c => c.Boolean(nullable: false));
            DropColumn("dbo.ItContractRoles", "IsLocallyAvaliable");
            DropColumn("dbo.ItSystemTypes", "IsLocallyAvaliable");
            DropColumn("dbo.BusinessTypes", "IsLocallyAvaliable");
            DropColumn("dbo.DataTypes", "IsLocallyAvaliable");
            DropColumn("dbo.FrequencyTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ArchiveTypes", "IsLocallyAvaliable");
            DropColumn("dbo.AgreementElementTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ItContractTemplateTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ItContractTypes", "IsLocallyAvaliable");
            DropColumn("dbo.GoalTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ItProjectTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ItProjectRoles", "IsLocallyAvaliable");
            DropColumn("dbo.OrganizationUnitRoles", "IsLocallyAvaliable");
            DropColumn("dbo.HandoverTrialTypes", "IsLocallyAvaliable");
            DropColumn("dbo.OptionExtendTypes", "IsLocallyAvaliable");
            DropColumn("dbo.PaymentFreqencyTypes", "IsLocallyAvaliable");
            DropColumn("dbo.PaymentModelTypes", "IsLocallyAvaliable");
            DropColumn("dbo.PriceRegulationTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ProcurementStrategyTypes", "IsLocallyAvaliable");
            DropColumn("dbo.PurchaseFormTypes", "IsLocallyAvaliable");
            DropColumn("dbo.TerminationDeadlineTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ItSystemRoles", "IsLocallyAvaliable");
            DropColumn("dbo.SensitiveDataTypes", "IsLocallyAvaliable");
            DropColumn("dbo.InterfaceTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ItInterfaceTypes", "IsLocallyAvaliable");
            DropColumn("dbo.MethodTypes", "IsLocallyAvaliable");
            DropColumn("dbo.TsaTypes", "IsLocallyAvaliable");
            DropColumn("dbo.ReportCategoryTypes", "IsLocallyAvaliable");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ReportCategoryTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.TsaTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.MethodTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItInterfaceTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.InterfaceTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.SensitiveDataTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemRoles", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.TerminationDeadlineTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.PurchaseFormTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProcurementStrategyTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.PriceRegulationTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentModelTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentFreqencyTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.OptionExtendTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.HandoverTrialTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.OrganizationUnitRoles", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectRoles", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.GoalTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTemplateTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.AgreementElementTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ArchiveTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.FrequencyTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.BusinessTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemTypes", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractRoles", "IsLocallyAvaliable", c => c.Boolean(nullable: false));
            DropColumn("dbo.ReportCategoryTypes", "IsLocallyAvailable");
            DropColumn("dbo.TsaTypes", "IsLocallyAvailable");
            DropColumn("dbo.MethodTypes", "IsLocallyAvailable");
            DropColumn("dbo.ItInterfaceTypes", "IsLocallyAvailable");
            DropColumn("dbo.InterfaceTypes", "IsLocallyAvailable");
            DropColumn("dbo.SensitiveDataTypes", "IsLocallyAvailable");
            DropColumn("dbo.ItSystemRoles", "IsLocallyAvailable");
            DropColumn("dbo.TerminationDeadlineTypes", "IsLocallyAvailable");
            DropColumn("dbo.PurchaseFormTypes", "IsLocallyAvailable");
            DropColumn("dbo.ProcurementStrategyTypes", "IsLocallyAvailable");
            DropColumn("dbo.PriceRegulationTypes", "IsLocallyAvailable");
            DropColumn("dbo.PaymentModelTypes", "IsLocallyAvailable");
            DropColumn("dbo.PaymentFreqencyTypes", "IsLocallyAvailable");
            DropColumn("dbo.OptionExtendTypes", "IsLocallyAvailable");
            DropColumn("dbo.HandoverTrialTypes", "IsLocallyAvailable");
            DropColumn("dbo.OrganizationUnitRoles", "IsLocallyAvailable");
            DropColumn("dbo.ItProjectRoles", "IsLocallyAvailable");
            DropColumn("dbo.ItProjectTypes", "IsLocallyAvailable");
            DropColumn("dbo.GoalTypes", "IsLocallyAvailable");
            DropColumn("dbo.ItContractTypes", "IsLocallyAvailable");
            DropColumn("dbo.ItContractTemplateTypes", "IsLocallyAvailable");
            DropColumn("dbo.AgreementElementTypes", "IsLocallyAvailable");
            DropColumn("dbo.ArchiveTypes", "IsLocallyAvailable");
            DropColumn("dbo.FrequencyTypes", "IsLocallyAvailable");
            DropColumn("dbo.DataTypes", "IsLocallyAvailable");
            DropColumn("dbo.BusinessTypes", "IsLocallyAvailable");
            DropColumn("dbo.ItSystemTypes", "IsLocallyAvailable");
            DropColumn("dbo.ItContractRoles", "IsLocallyAvailable");
        }
    }
}
