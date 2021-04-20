using System.Data.Entity;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Infrastructure.DataAccess.Mapping;
using System;
using System.Data.Entity.ModelConfiguration.Conventions;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.AdviceSent;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.KLE;
using Core.DomainModel.Qa.References;
using Core.DomainModel.SSO;

namespace Infrastructure.DataAccess
{
    public class KitosContext : DbContext
    {
        public KitosContext() : this("KitosContext") { }

        public KitosContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Database.Log = null;
        }

        public DbSet<ItContractAgreementElementTypes> ItContractAgreementElementTypes { get; set; }
        public DbSet<OrganizationRight> OrganizationRights { get; set; }
        public DbSet<Core.DomainModel.Advice.Advice> Advices { get; set; }
        public DbSet<AgreementElementType> AgreementElementTypes { get; set; }
        public DbSet<ArchiveType> ArchiveTypes { get; set; }
        public DbSet<ArchiveLocation> ArchiveLocation { get; set; }
        public DbSet<ArchiveTestLocation> ArchiveTestLocation { get; set; }
        public DbSet<BusinessType> BusinessTypes { get; set; }
        public DbSet<ReportCategoryType> ReportCategoryTypes { get; set; }
        public DbSet<Communication> Communications { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<GlobalConfig> GlobalConfigs { get; set; }
        public DbSet<ItContractTemplateType> ItContractTemplateTypes { get; set; }
        public DbSet<ItContractType> ItContractTypes { get; set; }
        public DbSet<DataType> DataTypes { get; set; }
        public DbSet<DataRow> DataRows { get; set; }
        public DbSet<EconomyYear> EconomyYears { get; set; }
        public DbSet<EconomyStream> EconomyStrams { get; set; }
        public DbSet<RelationFrequencyType> RelationFrequencyTypes { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<GoalStatus> GoalStatus { get; set; }
        public DbSet<GoalType> GoalTypes { get; set; }
        public DbSet<Handover> Handovers { get; set; }
        public DbSet<HandoverTrial> HandoverTrials { get; set; }
        public DbSet<HandoverTrialType> HandoverTrialTypes { get; set; }
        public DbSet<InterfaceType> InterfaceTypes { get; set; }
        public DbSet<ItInterfaceExhibit> ItInterfaceExhibits { get; set; }
        public DbSet<ItContract> ItContracts { get; set; }
        public DbSet<ItContractItSystemUsage> ItContractItSystemUsages { get; set; }
        public DbSet<ItContractRight> ItContractRights { get; set; }
        public DbSet<ItContractRole> ItContractRoles { get; set; }
        public DbSet<ItProject> ItProjects { get; set; }
        public DbSet<ItProjectStatus> ItProjectStatuses { get; set; }
        public DbSet<ItProjectStatusUpdate> ItProjectStatusUpdates { get; set; }
        public DbSet<ItProjectRight> ItProjectRights { get; set; }
        public DbSet<ItProjectRole> ItProjectRoles { get; set; }
        public DbSet<ItProjectOrgUnitUsage> ItProjectOrgUnitUsages { get; set; }
        public DbSet<ItSystemUsageOrgUnitUsage> ItSystemUsageOrgUnitUsages { get; set; }
        public DbSet<ItSystem> ItSystems { get; set; }
        public DbSet<ItSystemUsage> ItSystemUsages { get; set; }
        public DbSet<ItSystemCategories> ItSystemCategories { get; set; }
        public DbSet<ItSystemRight> ItSystemRights { get; set; }
        public DbSet<ItSystemRole> ItSystemRoles { get; set; }
        public DbSet<OptionExtendType> OptionExtendTypes { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationType> OrganizationTypes { get; set; }
        public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
        public DbSet<OrganizationUnitRight> OrganizationUnitRights { get; set; }
        public DbSet<OrganizationUnitRole> OrganizationUnitRoles { get; set; }
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }
        public DbSet<PaymentFreqencyType> PaymentFreqencyTypes { get; set; }
        public DbSet<PaymentMilestone> PaymentMilestones { get; set; }
        public DbSet<PaymentModelType> PaymentModelTypes { get; set; }
        public DbSet<PriceRegulationType> PriceRegulationTypes { get; set; }
        public DbSet<ProcurementStrategyType> ProcurementStrategyTypes { get; set; }
        public DbSet<ItProjectType> ItProjectTypes { get; set; }
        public DbSet<PurchaseFormType> PurchaseFormTypes { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Risk> Risks { get; set; }
        public DbSet<SensitiveDataType> SensitiveDataTypes { get; set; }
        public DbSet<Stakeholder> Stakeholders { get; set; }
        public DbSet<TerminationDeadlineType> TerminationDeadlineTypes { get; set; }
        public DbSet<TaskRef> TaskRefs { get; set; }
        public DbSet<AccessType> AccessTypes { get; set; }
        public DbSet<TaskUsage> TaskUsages { get; set; }
        public DbSet<Text> Texts { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ArchivePeriod> ArchivePeriods { get; set; }
        public DbSet<LocalAgreementElementType> LocalAgreementElementTypes { get; set; }
        public DbSet<LocalArchiveType> LocalArchiveTypes { get; set; }
        public DbSet<LocalArchiveLocation> LocalArchiveLocation { get; set; }
        public DbSet<LocalArchiveTestLocation> LocalArchiveTestLocation { get; set; }
        public DbSet<LocalBusinessType> LocalBusinessTypes { get; set; }
        public DbSet<LocalDataType> LocalDataTypes { get; set; }
        public DbSet<LocalRelationFrequencyType> LocalRelationFrequencyTypes { get; set; }
        public DbSet<LocalGoalType> LocalGoalTypes { get; set; }
        public DbSet<LocalHandoverTrialType> LocalHandoverTrialTypes { get; set; }
        public DbSet<LocalInterfaceType> LocalInterfaceTypes { get; set; }
        public DbSet<LocalItContractRole> LocalItContractRoles { get; set; }
        public DbSet<LocalItContractTemplateType> LocalItContractTemplateTypes { get; set; }
        public DbSet<LocalItContractType> LocalItContractTypes { get; set; }
        public DbSet<LocalItProjectRole> LocalItProjectRoles { get; set; }
        public DbSet<LocalItProjectType> LocalItProjectTypes { get; set; }
        public DbSet<LocalItSystemRole> LocalItSystemRoles { get; set; }
        public DbSet<LocalItSystemCategories> LocalItSystemCategories { get; set; }
        public DbSet<LocalOptionExtendType> LocalOptionExtendTypes { get; set; }
        public DbSet<LocalPaymentFreqencyType> LocalPaymentFreqencyTypes { get; set; }
        public DbSet<LocalPaymentModelType> LocalPaymentModelTypes { get; set; }
        public DbSet<LocalPriceRegulationType> LocalPriceRegulationTypes { get; set; }
        public DbSet<LocalProcurementStrategyType> LocalProcurementStrategyTypes { get; set; }
        public DbSet<LocalPurchaseFormType> LocalPurchaseFormTypes { get; set; }
        public DbSet<LocalReportCategoryType> LocalReportCategoryTypes { get; set; }
        public DbSet<LocalSensitiveDataType> LocalSensitiveDataTypes { get; set; }
        public DbSet<LocalTerminationDeadlineType> LocalTerminationDeadlineTypes { get; set; }
        public DbSet<LocalSensitivePersonalDataType> LocalSensitivePersonalDataTypes { get; set; }
        public DbSet<ExternalReference> ExternalReferences { get; set; }
        public DbSet<HelpText> HelpTexts { get; set; }
        public DbSet<LocalOrganizationUnitRole> LocalOrganizationUnitRoles { get; set; }
        public DbSet<AdviceSent> AdviceSent { get; set; }
        public DbSet<AttachedOption> AttachedOptions { get; set; }
        public DbSet<SensitivePersonalDataType> SensitivePersonalDataTypes { get; set; }
        public DbSet<DataResponsible> DataResponsibles { get; set; }
        public DbSet<DataProtectionAdvisor> DataProtectionAdvisors { get; set; }
        public DbSet<RegisterType> RegisterTypes { get; set; }
        public DbSet<LocalRegisterType> LocalRegisterTypes { get; set; }
        public DbSet<ContactPerson> ContactPersons { get; set; }
        public DbSet<KLEUpdateHistoryItem> KLEUpdateHistoryItems { get; set; }
        public DbSet<SystemRelation> SystemRelations { get; set; }
        public DbSet<BrokenExternalReferencesReport> BrokenExternalReferencesReports { get; set; }
        public DbSet<ItSystemUsageSensitiveDataLevel> ItSystemUsageSensitiveDataLevels { get; set; }
        public DbSet<SsoUserIdentity> SsoUserIdentities { get; set; }
        public DbSet<SsoOrganizationIdentity> SsoOrganizationIdentities { get; set; }
        public DbSet<DataProcessingRegistration> DataProcessingRegistrations { get; set; }
        public DbSet<DataProcessingRegistrationRole> DataProcessingRegistrationRoles { get; set; }
        public DbSet<LocalDataProcessingRegistrationRole> LocalDataProcessingRegistrationRoles { get; set; }
        public DbSet<DataProcessingRegistrationRight> DataProcessingRegistrationRights { get; set; }
        public DbSet<DataProcessingRegistrationReadModel> DataProcessingRegistrationReadModels { get; set; }
        public DbSet<DataProcessingRegistrationRoleAssignmentReadModel> DataProcessingRegistrationRoleAssignmentReadModels { get; set; }
        public DbSet<PendingReadModelUpdate> PendingReadModelUpdates { get; set; }
        public DbSet<DataProcessingBasisForTransferOption> DataProcessingBasisForTransferOptions { get; set; }
        public DbSet<DataProcessingOversightOption> DataProcessingOversightOptions { get; set; }
        public DbSet<DataProcessingDataResponsibleOption> DataProcessingDataResponsibleOptions { get; set; }
        public DbSet<DataProcessingCountryOption> DataProcessingCountryOptions { get; set; }
        public DbSet<LocalDataProcessingBasisForTransferOption> LocalDataProcessingBasisForTransferOptions { get; set; }
        public DbSet<LocalDataProcessingOversightOption> LocalDataProcessingOversightOptions { get; set; }
        public DbSet<LocalDataProcessingDataResponsibleOption> LocalDataProcessingDataResponsibleOptions { get; set; }
        public DbSet<LocalDataProcessingCountryOption> LocalDataProcessingCountryOptions { get; set; }
        public DbSet<ItSystemUsageOverviewReadModel> ItSystemUsageOverviewReadModels { get; set; }
        public DbSet<ItSystemUsageOverviewRoleAssignmentReadModel> ItSystemUsageOverviewRoleAssignmentReadModels { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // it's not possible to remove individual cascading deletes pr M:M relation
            // so have to remove all of them
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            // configure DateTime to use datetime2 in sql server as their range match
            modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));
            // placed first because then we have the ability to override
            base.OnModelCreating(modelBuilder);

            modelBuilder.ComplexType<ItProjectPhase>();

            modelBuilder.Configurations.Add(new AdviceMap());
            modelBuilder.Configurations.Add(new AgreementElementTypeMap());
            modelBuilder.Configurations.Add(new ArchiveTypeMap());
            modelBuilder.Configurations.Add(new BusinessTypeMap());
            modelBuilder.Configurations.Add(new CommunicationMap());
            modelBuilder.Configurations.Add(new ConfigMap());
            modelBuilder.Configurations.Add(new ItContractTemplateMap());
            modelBuilder.Configurations.Add(new ItContractTypeMap());
            modelBuilder.Configurations.Add(new DataTypeMap());
            modelBuilder.Configurations.Add(new DataRowMap());
            modelBuilder.Configurations.Add(new EconomyStreamMap());
            modelBuilder.Configurations.Add(new EconomyYearMap());
            modelBuilder.Configurations.Add(new RelationFrequencyTypeMap());
            modelBuilder.Configurations.Add(new GoalMap());
            modelBuilder.Configurations.Add(new GoalStatusMap());
            modelBuilder.Configurations.Add(new GoalTypeMap());
            modelBuilder.Configurations.Add(new HandoverMap());
            modelBuilder.Configurations.Add(new HandoverTrialMap());
            modelBuilder.Configurations.Add(new HandoverTrialTypeMap());
            modelBuilder.Configurations.Add(new InterfaceTypeMap());
            modelBuilder.Configurations.Add(new ItInterfaceMap());
            modelBuilder.Configurations.Add(new ItInterfaceExhibitMap());
            modelBuilder.Configurations.Add(new ItContractMap());

            modelBuilder.Configurations.Add(new ItContractRightMap());
            modelBuilder.Configurations.Add(new ItContractRoleMap());
            modelBuilder.Configurations.Add(new ItProjectMap());
            modelBuilder.Configurations.Add(new ItProjectStatusMap());
            modelBuilder.Configurations.Add(new ItProjectRightMap());
            modelBuilder.Configurations.Add(new ItProjectRoleMap());
            modelBuilder.Configurations.Add(new ItProjectOrgUnitUsageMap());
            modelBuilder.Configurations.Add(new ItSystemUsageOrgUnitUsageMap());
            modelBuilder.Configurations.Add(new ItSystemMap());
            modelBuilder.Configurations.Add(new ItSystemUsageMap());
            modelBuilder.Configurations.Add(new ItSystemRightMap());
            modelBuilder.Configurations.Add(new ItSystemRoleMap());
            modelBuilder.Configurations.Add(new OrganizationMap());
            modelBuilder.Configurations.Add(new OrganizationRightMap());
            modelBuilder.Configurations.Add(new OrganizationTypeMap());
            modelBuilder.Configurations.Add(new OrganizationUnitMap());
            modelBuilder.Configurations.Add(new OrganizationUnitRightMap());
            modelBuilder.Configurations.Add(new OrganizationUnitRoleMap());
            modelBuilder.Configurations.Add(new PasswordResetRequestMap());
            modelBuilder.Configurations.Add(new ItProjectTypeMap());
            modelBuilder.Configurations.Add(new ProcurementStrategyTypeMap());
            modelBuilder.Configurations.Add(new PurchaseFormTypeMap());
            modelBuilder.Configurations.Add(new ReportMap());
            modelBuilder.Configurations.Add(new RiskMap());
            modelBuilder.Configurations.Add(new SensitiveDataTypeMap());
            modelBuilder.Configurations.Add(new StakeholderMap());
            modelBuilder.Configurations.Add(new TaskRefMap());
            modelBuilder.Configurations.Add(new TaskUsageMap());
            modelBuilder.Configurations.Add(new TextMap());
            modelBuilder.Configurations.Add(new TerminationDeadlineTypeMap());
            modelBuilder.Configurations.Add(new UserMap());
            modelBuilder.Configurations.Add(new ArchivePeriodMap());
            modelBuilder.Configurations.Add(new PriceRegulationTypeMap());
            modelBuilder.Configurations.Add(new PaymentModelTypeMap());
            modelBuilder.Configurations.Add(new PaymentFreqencyTypeMap());
            modelBuilder.Configurations.Add(new OptionExtendTypeMap());
            modelBuilder.Configurations.Add(new ItContractItSystemUsageMap());
            modelBuilder.Configurations.Add(new ItContractAgreementElementTypeMap());
            modelBuilder.Configurations.Add(new DataResponsibleMap());
            modelBuilder.Configurations.Add(new DataProtectionAdvisorMap());
            modelBuilder.Configurations.Add(new SystemRelationMap());
            modelBuilder.Configurations.Add(new BrokenExternalReferencesReportMap());
            modelBuilder.Configurations.Add(new BrokenLinkInExternalReferenceMap());
            modelBuilder.Configurations.Add(new BrokenLinkInInterfaceMap());
            modelBuilder.Configurations.Add(new ItSystemUsageSensitiveDataLevelMap());
            modelBuilder.Configurations.Add(new SsoUserIdentityMap());
            modelBuilder.Configurations.Add(new SsoOrganizationIdentityMap());
            modelBuilder.Configurations.Add(new DataProcessingRegistrationMap());
            modelBuilder.Configurations.Add(new DataProcessingRegistrationRightMap());
            modelBuilder.Configurations.Add(new DataProcessingRegistrationRoleMap());
            modelBuilder.Configurations.Add(new DataProcessingRegistrationReadModelMap());
            modelBuilder.Configurations.Add(new DataProcessingRegistrationRoleAssignmentReadModelMap());
            modelBuilder.Configurations.Add(new DataProcessingBasisForTransferOptionMap());
            modelBuilder.Configurations.Add(new DataProcessingOversightOptionMap());
            modelBuilder.Configurations.Add(new DataProcessingDataResponsibleOptionMap());
            modelBuilder.Configurations.Add(new DataProcessingCountryOptionMap());
            modelBuilder.Configurations.Add(new ItSystemUsageOverviewReadModelMap());
            modelBuilder.Configurations.Add(new ItSystemUsageOverviewRoleAssignmentReadModelMap());
            modelBuilder.Configurations.Add(new ItSystemUsageOverviewTaskRefReadModelMap());
            modelBuilder.Configurations.Add(new ItSystemUsageOverviewSensitiveDataLevelReadModelMap());
            modelBuilder.Configurations.Add(new ItSystemUsageOverviewItProjectReadModelMap());
        }
    }
}
