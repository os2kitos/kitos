namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedPriorityforOptionEntity : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContractRoles", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.BusinessTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.DataTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.FrequencyTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.ArchiveTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.AgreementElementTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.ItContractTemplateTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.ItContractTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.GoalTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.ItProjectTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.ItProjectRoles", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.OrganizationUnitRoles", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.HandoverTrialTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.OptionExtendTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentFreqencyTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.PaymentModelTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.PriceRegulationTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.ProcurementStrategyTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.PurchaseFormTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.TerminationDeadlineTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.ItSystemRoles", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.SensitiveDataTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.InterfaceTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.ItInterfaceTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.MethodTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.TsaTypes", "Priority", c => c.Int(nullable: false));
            AddColumn("dbo.ReportCategoryTypes", "Priority", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReportCategoryTypes", "Priority");
            DropColumn("dbo.TsaTypes", "Priority");
            DropColumn("dbo.MethodTypes", "Priority");
            DropColumn("dbo.ItInterfaceTypes", "Priority");
            DropColumn("dbo.InterfaceTypes", "Priority");
            DropColumn("dbo.SensitiveDataTypes", "Priority");
            DropColumn("dbo.ItSystemRoles", "Priority");
            DropColumn("dbo.TerminationDeadlineTypes", "Priority");
            DropColumn("dbo.PurchaseFormTypes", "Priority");
            DropColumn("dbo.ProcurementStrategyTypes", "Priority");
            DropColumn("dbo.PriceRegulationTypes", "Priority");
            DropColumn("dbo.PaymentModelTypes", "Priority");
            DropColumn("dbo.PaymentFreqencyTypes", "Priority");
            DropColumn("dbo.OptionExtendTypes", "Priority");
            DropColumn("dbo.HandoverTrialTypes", "Priority");
            DropColumn("dbo.OrganizationUnitRoles", "Priority");
            DropColumn("dbo.ItProjectRoles", "Priority");
            DropColumn("dbo.ItProjectTypes", "Priority");
            DropColumn("dbo.GoalTypes", "Priority");
            DropColumn("dbo.ItContractTypes", "Priority");
            DropColumn("dbo.ItContractTemplateTypes", "Priority");
            DropColumn("dbo.AgreementElementTypes", "Priority");
            DropColumn("dbo.ArchiveTypes", "Priority");
            DropColumn("dbo.FrequencyTypes", "Priority");
            DropColumn("dbo.DataTypes", "Priority");
            DropColumn("dbo.BusinessTypes", "Priority");
            DropColumn("dbo.ItSystemTypes", "Priority");
            DropColumn("dbo.ItContractRoles", "Priority");
        }
    }
}
