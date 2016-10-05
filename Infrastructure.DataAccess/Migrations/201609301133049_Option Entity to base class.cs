namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OptionEntitytobaseclass : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContractRoles", "Description", c => c.String());
            AddColumn("dbo.ItSystemTypes", "Description", c => c.String());
            AddColumn("dbo.BusinessTypes", "Description", c => c.String());
            AddColumn("dbo.DataTypes", "Description", c => c.String());
            AddColumn("dbo.FrequencyTypes", "Description", c => c.String());
            AddColumn("dbo.ArchiveTypes", "Description", c => c.String());
            AddColumn("dbo.AgreementElementTypes", "Description", c => c.String());
            AddColumn("dbo.ItContractTemplateTypes", "Description", c => c.String());
            AddColumn("dbo.ItContractTypes", "Description", c => c.String());
            AddColumn("dbo.GoalTypes", "Description", c => c.String());
            AddColumn("dbo.ItProjectTypes", "Description", c => c.String());
            AddColumn("dbo.ItProjectRoles", "Description", c => c.String());
            AddColumn("dbo.OrganizationUnitRoles", "Description", c => c.String());
            AddColumn("dbo.HandoverTrialTypes", "Description", c => c.String());
            AddColumn("dbo.OptionExtendTypes", "Description", c => c.String());
            AddColumn("dbo.PaymentFreqencyTypes", "Description", c => c.String());
            AddColumn("dbo.PaymentModelTypes", "Description", c => c.String());
            AddColumn("dbo.PriceRegulationTypes", "Description", c => c.String());
            AddColumn("dbo.ProcurementStrategyTypes", "Description", c => c.String());
            AddColumn("dbo.PurchaseFormTypes", "Description", c => c.String());
            AddColumn("dbo.TerminationDeadlineTypes", "Description", c => c.String());
            AddColumn("dbo.ItSystemRoles", "Description", c => c.String());
            AddColumn("dbo.SensitiveDataTypes", "Description", c => c.String());
            AddColumn("dbo.InterfaceTypes", "Description", c => c.String());
            AddColumn("dbo.ItInterfaceTypes", "Description", c => c.String());
            AddColumn("dbo.MethodTypes", "Description", c => c.String());
            AddColumn("dbo.TsaTypes", "Description", c => c.String());
            AddColumn("dbo.ReportCategoryTypes", "Description", c => c.String());
            DropColumn("dbo.ItContractRoles", "Note");
            DropColumn("dbo.ItSystemTypes", "Note");
            DropColumn("dbo.BusinessTypes", "Note");
            DropColumn("dbo.DataTypes", "Note");
            DropColumn("dbo.FrequencyTypes", "Note");
            DropColumn("dbo.ArchiveTypes", "Note");
            DropColumn("dbo.AgreementElementTypes", "Note");
            DropColumn("dbo.ItContractTemplateTypes", "Note");
            DropColumn("dbo.ItContractTypes", "Note");
            DropColumn("dbo.GoalTypes", "Note");
            DropColumn("dbo.ItProjectTypes", "Note");
            DropColumn("dbo.ItProjectRoles", "Note");
            DropColumn("dbo.OrganizationUnitRoles", "Note");
            DropColumn("dbo.HandoverTrialTypes", "Note");
            DropColumn("dbo.OptionExtendTypes", "Note");
            DropColumn("dbo.PaymentFreqencyTypes", "Note");
            DropColumn("dbo.PaymentModelTypes", "Note");
            DropColumn("dbo.PriceRegulationTypes", "Note");
            DropColumn("dbo.ProcurementStrategyTypes", "Note");
            DropColumn("dbo.PurchaseFormTypes", "Note");
            DropColumn("dbo.TerminationDeadlineTypes", "Note");
            DropColumn("dbo.ItSystemRoles", "Note");
            DropColumn("dbo.SensitiveDataTypes", "Note");
            DropColumn("dbo.InterfaceTypes", "Note");
            DropColumn("dbo.ItInterfaceTypes", "Note");
            DropColumn("dbo.MethodTypes", "Note");
            DropColumn("dbo.TsaTypes", "Note");
            DropColumn("dbo.ReportCategoryTypes", "Note");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ReportCategoryTypes", "Note", c => c.String());
            AddColumn("dbo.TsaTypes", "Note", c => c.String());
            AddColumn("dbo.MethodTypes", "Note", c => c.String());
            AddColumn("dbo.ItInterfaceTypes", "Note", c => c.String());
            AddColumn("dbo.InterfaceTypes", "Note", c => c.String());
            AddColumn("dbo.SensitiveDataTypes", "Note", c => c.String());
            AddColumn("dbo.ItSystemRoles", "Note", c => c.String());
            AddColumn("dbo.TerminationDeadlineTypes", "Note", c => c.String());
            AddColumn("dbo.PurchaseFormTypes", "Note", c => c.String());
            AddColumn("dbo.ProcurementStrategyTypes", "Note", c => c.String());
            AddColumn("dbo.PriceRegulationTypes", "Note", c => c.String());
            AddColumn("dbo.PaymentModelTypes", "Note", c => c.String());
            AddColumn("dbo.PaymentFreqencyTypes", "Note", c => c.String());
            AddColumn("dbo.OptionExtendTypes", "Note", c => c.String());
            AddColumn("dbo.HandoverTrialTypes", "Note", c => c.String());
            AddColumn("dbo.OrganizationUnitRoles", "Note", c => c.String());
            AddColumn("dbo.ItProjectRoles", "Note", c => c.String());
            AddColumn("dbo.ItProjectTypes", "Note", c => c.String());
            AddColumn("dbo.GoalTypes", "Note", c => c.String());
            AddColumn("dbo.ItContractTypes", "Note", c => c.String());
            AddColumn("dbo.ItContractTemplateTypes", "Note", c => c.String());
            AddColumn("dbo.AgreementElementTypes", "Note", c => c.String());
            AddColumn("dbo.ArchiveTypes", "Note", c => c.String());
            AddColumn("dbo.FrequencyTypes", "Note", c => c.String());
            AddColumn("dbo.DataTypes", "Note", c => c.String());
            AddColumn("dbo.BusinessTypes", "Note", c => c.String());
            AddColumn("dbo.ItSystemTypes", "Note", c => c.String());
            AddColumn("dbo.ItContractRoles", "Note", c => c.String());
            DropColumn("dbo.ReportCategoryTypes", "Description");
            DropColumn("dbo.TsaTypes", "Description");
            DropColumn("dbo.MethodTypes", "Description");
            DropColumn("dbo.ItInterfaceTypes", "Description");
            DropColumn("dbo.InterfaceTypes", "Description");
            DropColumn("dbo.SensitiveDataTypes", "Description");
            DropColumn("dbo.ItSystemRoles", "Description");
            DropColumn("dbo.TerminationDeadlineTypes", "Description");
            DropColumn("dbo.PurchaseFormTypes", "Description");
            DropColumn("dbo.ProcurementStrategyTypes", "Description");
            DropColumn("dbo.PriceRegulationTypes", "Description");
            DropColumn("dbo.PaymentModelTypes", "Description");
            DropColumn("dbo.PaymentFreqencyTypes", "Description");
            DropColumn("dbo.OptionExtendTypes", "Description");
            DropColumn("dbo.HandoverTrialTypes", "Description");
            DropColumn("dbo.OrganizationUnitRoles", "Description");
            DropColumn("dbo.ItProjectRoles", "Description");
            DropColumn("dbo.ItProjectTypes", "Description");
            DropColumn("dbo.GoalTypes", "Description");
            DropColumn("dbo.ItContractTypes", "Description");
            DropColumn("dbo.ItContractTemplateTypes", "Description");
            DropColumn("dbo.AgreementElementTypes", "Description");
            DropColumn("dbo.ArchiveTypes", "Description");
            DropColumn("dbo.FrequencyTypes", "Description");
            DropColumn("dbo.DataTypes", "Description");
            DropColumn("dbo.BusinessTypes", "Description");
            DropColumn("dbo.ItSystemTypes", "Description");
            DropColumn("dbo.ItContractRoles", "Description");
        }
    }
}
