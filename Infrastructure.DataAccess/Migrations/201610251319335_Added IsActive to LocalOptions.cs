namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedIsActivetoLocalOptions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LocalAgreementElementTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalArchiveTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalBusinessTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalDataTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalFrequencyTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalGoalTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalHandoverTrialTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalInterfaceTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalItContractRoles", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalItContractTemplateTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalItContractTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalItInterfaceTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalItProjectRoles", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalItProjectTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalItSystemRoles", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalItSystemTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalMethodTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalOptionExtendTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalPaymentFreqencyTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalPaymentModelTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalPriceRegulationTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalProcurementStrategyTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalPurchaseFormTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalReportCategoryTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalSensitiveDataTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalTerminationDeadlineTypes", "IsActive", c => c.Boolean(nullable: false));
            AddColumn("dbo.LocalTsaTypes", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.LocalTsaTypes", "IsActive");
            DropColumn("dbo.LocalTerminationDeadlineTypes", "IsActive");
            DropColumn("dbo.LocalSensitiveDataTypes", "IsActive");
            DropColumn("dbo.LocalReportCategoryTypes", "IsActive");
            DropColumn("dbo.LocalPurchaseFormTypes", "IsActive");
            DropColumn("dbo.LocalProcurementStrategyTypes", "IsActive");
            DropColumn("dbo.LocalPriceRegulationTypes", "IsActive");
            DropColumn("dbo.LocalPaymentModelTypes", "IsActive");
            DropColumn("dbo.LocalPaymentFreqencyTypes", "IsActive");
            DropColumn("dbo.LocalOptionExtendTypes", "IsActive");
            DropColumn("dbo.LocalMethodTypes", "IsActive");
            DropColumn("dbo.LocalItSystemTypes", "IsActive");
            DropColumn("dbo.LocalItSystemRoles", "IsActive");
            DropColumn("dbo.LocalItProjectTypes", "IsActive");
            DropColumn("dbo.LocalItProjectRoles", "IsActive");
            DropColumn("dbo.LocalItInterfaceTypes", "IsActive");
            DropColumn("dbo.LocalItContractTypes", "IsActive");
            DropColumn("dbo.LocalItContractTemplateTypes", "IsActive");
            DropColumn("dbo.LocalItContractRoles", "IsActive");
            DropColumn("dbo.LocalInterfaceTypes", "IsActive");
            DropColumn("dbo.LocalHandoverTrialTypes", "IsActive");
            DropColumn("dbo.LocalGoalTypes", "IsActive");
            DropColumn("dbo.LocalFrequencyTypes", "IsActive");
            DropColumn("dbo.LocalDataTypes", "IsActive");
            DropColumn("dbo.LocalBusinessTypes", "IsActive");
            DropColumn("dbo.LocalArchiveTypes", "IsActive");
            DropColumn("dbo.LocalAgreementElementTypes", "IsActive");
        }
    }
}
