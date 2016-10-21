namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class priorityadded : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ItContractRoles", "priority");
            DropColumn("dbo.ItSystemTypes", "priority");
            DropColumn("dbo.BusinessTypes", "priority");
            DropColumn("dbo.DataTypes", "priority");
            DropColumn("dbo.FrequencyTypes", "priority");
            DropColumn("dbo.ArchiveTypes", "priority");
            DropColumn("dbo.AgreementElementTypes", "priority");
            DropColumn("dbo.ItContractTemplateTypes", "priority");
            DropColumn("dbo.ItContractTypes", "priority");
            DropColumn("dbo.GoalTypes", "priority");
            DropColumn("dbo.ItProjectTypes", "priority");
            DropColumn("dbo.ItProjectRoles", "priority");
            DropColumn("dbo.OrganizationUnitRoles", "priority");
            DropColumn("dbo.HandoverTrialTypes", "priority");
            DropColumn("dbo.OptionExtendTypes", "priority");
            DropColumn("dbo.PaymentFreqencyTypes", "priority");
            DropColumn("dbo.PaymentModelTypes", "priority");
            DropColumn("dbo.PriceRegulationTypes", "priority");
            DropColumn("dbo.ProcurementStrategyTypes", "priority");
            DropColumn("dbo.PurchaseFormTypes", "priority");
            DropColumn("dbo.TerminationDeadlineTypes", "priority");
            DropColumn("dbo.ItSystemRoles", "priority");
            DropColumn("dbo.SensitiveDataTypes", "priority");
            DropColumn("dbo.InterfaceTypes", "priority");
            DropColumn("dbo.ItInterfaceTypes", "priority");
            DropColumn("dbo.MethodTypes", "priority");
            DropColumn("dbo.TsaTypes", "priority");
            DropColumn("dbo.ReportCategoryTypes", "priority");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ReportCategoryTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.TsaTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.MethodTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.ItInterfaceTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.InterfaceTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.SensitiveDataTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemRoles", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.TerminationDeadlineTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.PurchaseFormTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.ProcurementStrategyTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.PriceRegulationTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentModelTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentFreqencyTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.OptionExtendTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.HandoverTrialTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.OrganizationUnitRoles", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.ItProjectRoles", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.ItProjectTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.GoalTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.ItContractTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.ItContractTemplateTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.AgreementElementTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.ArchiveTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.FrequencyTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.DataTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.BusinessTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemTypes", "priority", c => c.Int(nullable: false));
            AddColumn("dbo.ItContractRoles", "priority", c => c.Int(nullable: false));
        }
    }
}
