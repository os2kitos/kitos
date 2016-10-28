namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIsObligatory : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItSystemTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.BusinessTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.DataTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.FrequencyTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.ArchiveTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.AgreementElementTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTemplateTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItContractTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.GoalTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItProjectRoles", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.OrganizationUnitRoles", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.HandoverTrialTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.OptionExtendTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentFreqencyTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentModelTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.PriceRegulationTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.ProcurementStrategyTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.PurchaseFormTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.TerminationDeadlineTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItSystemRoles", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.SensitiveDataTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.InterfaceTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.ItInterfaceTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.MethodTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.TsaTypes", "IsObligatory", c => c.Boolean(nullable: false));
            AddColumn("dbo.ReportCategoryTypes", "IsObligatory", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ReportCategoryTypes", "IsObligatory");
            DropColumn("dbo.TsaTypes", "IsObligatory");
            DropColumn("dbo.MethodTypes", "IsObligatory");
            DropColumn("dbo.ItInterfaceTypes", "IsObligatory");
            DropColumn("dbo.InterfaceTypes", "IsObligatory");
            DropColumn("dbo.SensitiveDataTypes", "IsObligatory");
            DropColumn("dbo.ItSystemRoles", "IsObligatory");
            DropColumn("dbo.TerminationDeadlineTypes", "IsObligatory");
            DropColumn("dbo.PurchaseFormTypes", "IsObligatory");
            DropColumn("dbo.ProcurementStrategyTypes", "IsObligatory");
            DropColumn("dbo.PriceRegulationTypes", "IsObligatory");
            DropColumn("dbo.PaymentModelTypes", "IsObligatory");
            DropColumn("dbo.PaymentFreqencyTypes", "IsObligatory");
            DropColumn("dbo.OptionExtendTypes", "IsObligatory");
            DropColumn("dbo.HandoverTrialTypes", "IsObligatory");
            DropColumn("dbo.OrganizationUnitRoles", "IsObligatory");
            DropColumn("dbo.ItProjectRoles", "IsObligatory");
            DropColumn("dbo.ItProjectTypes", "IsObligatory");
            DropColumn("dbo.GoalTypes", "IsObligatory");
            DropColumn("dbo.ItContractTypes", "IsObligatory");
            DropColumn("dbo.ItContractTemplateTypes", "IsObligatory");
            DropColumn("dbo.AgreementElementTypes", "IsObligatory");
            DropColumn("dbo.ArchiveTypes", "IsObligatory");
            DropColumn("dbo.FrequencyTypes", "IsObligatory");
            DropColumn("dbo.DataTypes", "IsObligatory");
            DropColumn("dbo.BusinessTypes", "IsObligatory");
            DropColumn("dbo.ItSystemTypes", "IsObligatory");
            DropColumn("dbo.ItContractRoles", "IsObligatory");
        }
    }
}
