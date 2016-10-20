namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedpropLocalOptionIdonOptionEntity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContractRoles", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.BusinessTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.DataTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.FrequencyTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.ArchiveTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.AgreementElementTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.ItContractTemplateTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.ItContractTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.GoalTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.ItProjectTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.ItProjectRoles", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.OrganizationUnitRoles", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.HandoverTrialTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.OptionExtendTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentFreqencyTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentModelTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.PriceRegulationTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.ProcurementStrategyTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.PurchaseFormTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.TerminationDeadlineTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemRoles", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.SensitiveDataTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.InterfaceTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.ItInterfaceTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.MethodTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.TsaTypes", "LocalOptionId", c => c.Int(nullable: false));
            AddColumn("dbo.ReportCategoryTypes", "LocalOptionId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReportCategoryTypes", "LocalOptionId");
            DropColumn("dbo.TsaTypes", "LocalOptionId");
            DropColumn("dbo.MethodTypes", "LocalOptionId");
            DropColumn("dbo.ItInterfaceTypes", "LocalOptionId");
            DropColumn("dbo.InterfaceTypes", "LocalOptionId");
            DropColumn("dbo.SensitiveDataTypes", "LocalOptionId");
            DropColumn("dbo.ItSystemRoles", "LocalOptionId");
            DropColumn("dbo.TerminationDeadlineTypes", "LocalOptionId");
            DropColumn("dbo.PurchaseFormTypes", "LocalOptionId");
            DropColumn("dbo.ProcurementStrategyTypes", "LocalOptionId");
            DropColumn("dbo.PriceRegulationTypes", "LocalOptionId");
            DropColumn("dbo.PaymentModelTypes", "LocalOptionId");
            DropColumn("dbo.PaymentFreqencyTypes", "LocalOptionId");
            DropColumn("dbo.OptionExtendTypes", "LocalOptionId");
            DropColumn("dbo.HandoverTrialTypes", "LocalOptionId");
            DropColumn("dbo.OrganizationUnitRoles", "LocalOptionId");
            DropColumn("dbo.ItProjectRoles", "LocalOptionId");
            DropColumn("dbo.ItProjectTypes", "LocalOptionId");
            DropColumn("dbo.GoalTypes", "LocalOptionId");
            DropColumn("dbo.ItContractTypes", "LocalOptionId");
            DropColumn("dbo.ItContractTemplateTypes", "LocalOptionId");
            DropColumn("dbo.AgreementElementTypes", "LocalOptionId");
            DropColumn("dbo.ArchiveTypes", "LocalOptionId");
            DropColumn("dbo.FrequencyTypes", "LocalOptionId");
            DropColumn("dbo.DataTypes", "LocalOptionId");
            DropColumn("dbo.BusinessTypes", "LocalOptionId");
            DropColumn("dbo.ItSystemTypes", "LocalOptionId");
            DropColumn("dbo.ItContractRoles", "LocalOptionId");
        }
    }
}
