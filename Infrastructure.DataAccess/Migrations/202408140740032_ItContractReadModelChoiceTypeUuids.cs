namespace Infrastructure.DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ItContractReadModelChoiceTypeUuids : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ItContractOverviewReadModels", "CriticalityUuid", c => c.Guid());
            AddColumn("dbo.ItContractOverviewReadModels", "ContractTypeUuid", c => c.Guid());
            AddColumn("dbo.ItContractOverviewReadModels", "ContractTemplateUuid", c => c.Guid());
            AddColumn("dbo.ItContractOverviewReadModels", "PurchaseFormUuid", c => c.Guid());
            AddColumn("dbo.ItContractOverviewReadModels", "ProcurementStrategyUuid", c => c.Guid());
            AddColumn("dbo.ItContractOverviewReadModels", "PaymentModelUuid", c => c.Guid());
            AddColumn("dbo.ItContractOverviewReadModels", "PaymentFrequencyUuid", c => c.Guid());
            AddColumn("dbo.ItContractOverviewReadModels", "OptionExtendUuid", c => c.Guid());
            AddColumn("dbo.ItContractOverviewReadModels", "TerminationDeadlineUuid", c => c.Guid());
            CreateIndex("dbo.ItContractOverviewReadModels", "CriticalityUuid", name: "IX_CriticalityType_Uuid");
            CreateIndex("dbo.ItContractOverviewReadModels", "ContractTypeUuid", name: "IX_ItContractType_Uuid");
            CreateIndex("dbo.ItContractOverviewReadModels", "ContractTemplateUuid", name: "IX_ItContractTemplateType_Uuid");
            CreateIndex("dbo.ItContractOverviewReadModels", "PurchaseFormUuid", name: "IX_PurchaseFormType_Uuid");
            CreateIndex("dbo.ItContractOverviewReadModels", "ProcurementStrategyUuid", name: "IX_ProcurementStrategyType_Uuid");
            CreateIndex("dbo.ItContractOverviewReadModels", "PaymentModelUuid", name: "IX_PaymentModelType_Uuid");
            CreateIndex("dbo.ItContractOverviewReadModels", "PaymentFrequencyUuid", name: "IX_PaymentFreqencyType_Uuid");
            CreateIndex("dbo.ItContractOverviewReadModels", "OptionExtendUuid", name: "IX_OptionExtendType_Uuid");
            CreateIndex("dbo.ItContractOverviewReadModels", "TerminationDeadlineUuid", name: "IX_TerminationDeadlineType_Uuid");
        }
        
        public override void Down()
        {
            DropIndex("dbo.ItContractOverviewReadModels", "IX_TerminationDeadlineType_Uuid");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_OptionExtendType_Uuid");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_PaymentFreqencyType_Uuid");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_PaymentModelType_Uuid");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_ProcurementStrategyType_Uuid");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_PurchaseFormType_Uuid");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_ItContractTemplateType_Uuid");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_ItContractType_Uuid");
            DropIndex("dbo.ItContractOverviewReadModels", "IX_CriticalityType_Uuid");
            DropColumn("dbo.ItContractOverviewReadModels", "TerminationDeadlineUuid");
            DropColumn("dbo.ItContractOverviewReadModels", "OptionExtendUuid");
            DropColumn("dbo.ItContractOverviewReadModels", "PaymentFrequencyUuid");
            DropColumn("dbo.ItContractOverviewReadModels", "PaymentModelUuid");
            DropColumn("dbo.ItContractOverviewReadModels", "ProcurementStrategyUuid");
            DropColumn("dbo.ItContractOverviewReadModels", "PurchaseFormUuid");
            DropColumn("dbo.ItContractOverviewReadModels", "ContractTemplateUuid");
            DropColumn("dbo.ItContractOverviewReadModels", "ContractTypeUuid");
            DropColumn("dbo.ItContractOverviewReadModels", "CriticalityUuid");
        }
    }
}
