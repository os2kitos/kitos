using System.Data.Entity;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Infrastructure.DataAccess.Mapping;
using System;
using System.Data.Entity.ModelConfiguration.Conventions;
using Core.DomainModel.Advice;
using Core.DomainModel.Organization;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.BackgroundJobs;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.ItContract.Read;
using Core.DomainModel.ItSystemUsage.GDPR;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.KendoConfig;
using Core.DomainModel.KLE;
using Core.DomainModel.Qa.References;
using Core.DomainModel.SSO;
using Core.DomainModel.Notification;
using Core.DomainModel.PublicMessage;
using Core.DomainModel.Tracking;
using Core.DomainModel.UIConfiguration;
using Infrastructure.DataAccess.Tools;

namespace Infrastructure.DataAccess
{
    public class KitosContext : DbContext
    {
        public KitosContext() : this(ConnectionStringTools.GetConnectionString("KitosContext")) { }
        
        public KitosContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Database.Log = null;
        }

        public DbSet<ItContractAgreementElementTypes> ItContractAgreementElementTypes { get; set; }
        public DbSet<OrganizationRight> OrganizationRights { get; set; }
        public DbSet<Core.DomainModel.Advice.Advice> Advices { get; set; }
        public DbSet<Core.DomainModel.Advice.AdviceUserRelation> AdviceUserRelations { get; set; }
        public DbSet<AgreementElementType> AgreementElementTypes { get; set; }
        public DbSet<ArchiveType> ArchiveTypes { get; set; }
        public DbSet<ArchiveLocation> ArchiveLocation { get; set; }
        public DbSet<ArchiveTestLocation> ArchiveTestLocation { get; set; }
        public DbSet<BusinessType> BusinessTypes { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<ItContractTemplateType> ItContractTemplateTypes { get; set; }
        public DbSet<ItContractType> ItContractTypes { get; set; }
        public DbSet<DataType> DataTypes { get; set; }
        public DbSet<DataRow> DataRows { get; set; }
        public DbSet<EconomyStream> EconomyStrams { get; set; }
        public DbSet<RelationFrequencyType> RelationFrequencyTypes { get; set; }
        public DbSet<CriticalityType> CriticalityTypes { get; set; }
        public DbSet<LocalCriticalityType> LocalCriticalityTypes { get; set; }
        public DbSet<InterfaceType> InterfaceTypes { get; set; }
        public DbSet<ItInterfaceExhibit> ItInterfaceExhibits { get; set; }
        public DbSet<ItContract> ItContracts { get; set; }
        public DbSet<ItContractItSystemUsage> ItContractItSystemUsages { get; set; }
        public DbSet<ItContractRight> ItContractRights { get; set; }
        public DbSet<ItContractRole> ItContractRoles { get; set; }
        public DbSet<ItSystemUsageOrgUnitUsage> ItSystemUsageOrgUnitUsages { get; set; }
        public DbSet<ItSystem> ItSystems { get; set; }
        public DbSet<ItSystemUsage> ItSystemUsages { get; set; }
        public DbSet<ItSystemUsagePersonalData> ItSystemUsagePersonalDataOptions { get; set; }
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
        public DbSet<PaymentModelType> PaymentModelTypes { get; set; }
        public DbSet<PriceRegulationType> PriceRegulationTypes { get; set; }
        public DbSet<ProcurementStrategyType> ProcurementStrategyTypes { get; set; }
        public DbSet<PurchaseFormType> PurchaseFormTypes { get; set; }
        public DbSet<SensitiveDataType> SensitiveDataTypes { get; set; }
        public DbSet<TerminationDeadlineType> TerminationDeadlineTypes { get; set; }
        public DbSet<TaskRef> TaskRefs { get; set; }
        public DbSet<Text> Texts { get; set; }
        public DbSet<PublicMessage> PublicMessages { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<ArchivePeriod> ArchivePeriods { get; set; }
        public DbSet<LocalAgreementElementType> LocalAgreementElementTypes { get; set; }
        public DbSet<LocalArchiveType> LocalArchiveTypes { get; set; }
        public DbSet<LocalArchiveLocation> LocalArchiveLocation { get; set; }
        public DbSet<LocalArchiveTestLocation> LocalArchiveTestLocation { get; set; }
        public DbSet<LocalBusinessType> LocalBusinessTypes { get; set; }
        public DbSet<LocalDataType> LocalDataTypes { get; set; }
        public DbSet<LocalRelationFrequencyType> LocalRelationFrequencyTypes { get; set; }
        public DbSet<LocalInterfaceType> LocalInterfaceTypes { get; set; }
        public DbSet<LocalItContractRole> LocalItContractRoles { get; set; }
        public DbSet<LocalItContractTemplateType> LocalItContractTemplateTypes { get; set; }
        public DbSet<LocalItContractType> LocalItContractTypes { get; set; }
        public DbSet<LocalItSystemRole> LocalItSystemRoles { get; set; }
        public DbSet<LocalItSystemCategories> LocalItSystemCategories { get; set; }
        public DbSet<LocalOptionExtendType> LocalOptionExtendTypes { get; set; }
        public DbSet<LocalPaymentFreqencyType> LocalPaymentFreqencyTypes { get; set; }
        public DbSet<LocalPaymentModelType> LocalPaymentModelTypes { get; set; }
        public DbSet<LocalPriceRegulationType> LocalPriceRegulationTypes { get; set; }
        public DbSet<LocalProcurementStrategyType> LocalProcurementStrategyTypes { get; set; }
        public DbSet<LocalPurchaseFormType> LocalPurchaseFormTypes { get; set; }
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
        public DbSet<StsOrganizationIdentity> SsoOrganizationIdentities { get; set; }
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
        public DbSet<ItSystemUsageOverviewTaskRefReadModel> ItSystemUsageOverviewTaskRefReadModels { get; set; }
        public DbSet<ItSystemUsageOverviewSensitiveDataLevelReadModel> ItSystemUsageOverviewSensitiveDataLevelReadModels { get; set; }
        public DbSet<ItSystemUsageOverviewArchivePeriodReadModel> ItSystemUsageOverviewArchivePeriodReadModels { get; set; }
        public DbSet<ItSystemUsageOverviewDataProcessingRegistrationReadModel> ItSystemUsageOverviewDataProcessingRegistrationReadModels { get; set; }
        public DbSet<ItSystemUsageOverviewInterfaceReadModel> ItSystemUsageOverviewInterfaceReadModels { get; set; }
        public DbSet<ItSystemUsageOverviewUsedBySystemUsageReadModel> ItSystemUsageOverviewItSystemUsageReadModels { get; set; }
        public DbSet<ItSystemUsageOverviewUsingSystemUsageReadModel> ItSystemUsageOverviewUsingSystemUsageReadModels { get; set; }
        public DbSet<KendoOrganizationalConfiguration> KendoOrganizationalConfigurations { get; set; }
        public DbSet<DataProcessingRegistrationOversightDate> DataProcessingRegistrationOversightDates { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<ItInterface> ItInterfaces { get; set; }
        public DbSet<LifeCycleTrackingEvent> LifeCycleTrackingEvents { get; set; }
        public DbSet<UIModuleCustomization> UIModuleCustomizations { get; set; }
        public DbSet<CustomizedUINode> CustomizedUiNodes { get; set; }
        public DbSet<ItContractOverviewReadModel> ItContractOverviewReadModels { get; set; }
        public DbSet<ItContractOverviewReadModelDataProcessingAgreement> ItContractOverviewReadModelDataProcessingAgreements { get; set; }
        public DbSet<ItContractOverviewReadModelItSystemUsage> ItContractOverviewReadModelItSystemUsages { get; set; }
        public DbSet<ItContractOverviewRoleAssignmentReadModel> ItContractOverviewRoleAssignmentReadModels { get; set; }
        public DbSet<ItContractOverviewReadModelSystemRelation> ItContractOverviewReadModelSystemRelations { get; set; }
        public DbSet<StsOrganizationConnection> StsOrganizationConnections { get; set; }
        public DbSet<StsOrganizationChangeLog> StsOrganizationChangeLogs { get; set; }
        public DbSet<StsOrganizationConsequenceLog> StsOrganizationConsequenceLogs { get; set; }
        public DbSet<SubDataProcessor> SubDataProcessors { get; set; }
        public DbSet<ItSystemUsageOverviewRelevantOrgUnitReadModel> ItSystemUsageOverviewRelevantOrgUnitReadModels { get; set; }
        public DbSet<ItSystemUsageOverviewItContractReadModel> ItSystemUsageOverviewItContractReadModels { get; set; }
        public DbSet<CountryCode> CountryCodes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // it's not possible to remove individual cascading deletes pr M:M relation
            // so have to remove all of them
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            // configure DateTime to use datetime2 in sql server as their range match
            modelBuilder.Properties<DateTime>().Configure(c => c.HasColumnType("datetime2"));
            // placed first because then we have the ability to override
            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new AdviceMap());
            modelBuilder.Configurations.Add(new AgreementElementTypeMap());
            modelBuilder.Configurations.Add(new ArchiveTypeMap());
            modelBuilder.Configurations.Add(new BusinessTypeMap());
            modelBuilder.Configurations.Add(new ConfigMap());
            modelBuilder.Configurations.Add(new ItContractTemplateMap());
            modelBuilder.Configurations.Add(new ItContractTypeMap());
            modelBuilder.Configurations.Add(new DataTypeMap());
            modelBuilder.Configurations.Add(new DataRowMap());
            modelBuilder.Configurations.Add(new EconomyStreamMap());
            modelBuilder.Configurations.Add(new RelationFrequencyTypeMap());
            modelBuilder.Configurations.Add(new CriticalityTypeMap());
            modelBuilder.Configurations.Add(new InterfaceTypeMap());
            modelBuilder.Configurations.Add(new ItInterfaceMap());
            modelBuilder.Configurations.Add(new ItInterfaceExhibitMap());
            modelBuilder.Configurations.Add(new ItContractMap());
            modelBuilder.Configurations.Add(new ItContractRightMap());
            modelBuilder.Configurations.Add(new ItContractRoleMap());
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
            modelBuilder.Configurations.Add(new ProcurementStrategyTypeMap());
            modelBuilder.Configurations.Add(new PurchaseFormTypeMap());
            modelBuilder.Configurations.Add(new SensitiveDataTypeMap());
            modelBuilder.Configurations.Add(new TaskRefMap());
            modelBuilder.Configurations.Add(new TextMap());
            modelBuilder.Configurations.Add(new PublicMessageMap());
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
            modelBuilder.Configurations.Add(new ItSystemUsagePersonalDataOptionsMap());
            modelBuilder.Configurations.Add(new SsoUserIdentityMap());
            modelBuilder.Configurations.Add(new StsOrganizationIdentityMap());
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
            modelBuilder.Configurations.Add(new ItSystemUsageOverviewArchivePeriodReadModelMap());
            modelBuilder.Configurations.Add(new ItSystemUsageOverviewDataProcessingRegistrationReadModelMap());
            modelBuilder.Configurations.Add(new PendingReadModelUpdateMap());
            modelBuilder.Configurations.Add(new ItSystemUsageOverviewInterfaceReadModelMap());
            modelBuilder.Configurations.Add(new ItSystemUsageOverviewUsedBySystemUsageReadModelMap());
            modelBuilder.Configurations.Add(new KendoOrganizationalConfigurationMap());
            modelBuilder.Configurations.Add(new DataProcessingRegistrationOversightDateMap());
            modelBuilder.Configurations.Add(new ItSystemUsageOverviewUsingSystemUsageReadModelMap());
            modelBuilder.Configurations.Add(new UserNotificationMap());
            modelBuilder.Configurations.Add(new AttachedOptionMap());
            modelBuilder.Configurations.Add(new LifeCycleTrackingEventMap());
            modelBuilder.Configurations.Add(new UIModuleCustomizationMap());
            modelBuilder.Configurations.Add(new CustomizedUINodeMap());
            modelBuilder.Configurations.Add(new AdviceUserRelationMap());
            modelBuilder.Configurations.Add(new ItContractOverviewReadModelMap());
            modelBuilder.Configurations.Add(new ItContractOverviewReadModelDataProcessingAgreementMap());
            modelBuilder.Configurations.Add(new ItContractOverviewReadModelItSystemUsageMap());
            modelBuilder.Configurations.Add(new ItContractOverviewRoleAssignmentReadModelMap());
            modelBuilder.Configurations.Add(new ItContractOverviewReadModelSystemRelationMap());
            modelBuilder.Configurations.Add(new StsOrganizationConnectionMap());
            modelBuilder.Configurations.Add(new StsOrganizationChangeLogMap());
            modelBuilder.Configurations.Add(new StsOrganizationConsequenceLogMap());
            modelBuilder.Configurations.Add(new SubDataProcessorMap());
            modelBuilder.Configurations.Add(new ExternalReferenceMap());
            modelBuilder.Configurations.Add(new ItSystemUsageOverviewRelevantOrgUnitReadModelMap());
            modelBuilder.Configurations.Add(new ItSystemUsageOverviewItContractReadModelMap());
            modelBuilder.Configurations.Add(new CountryCodeMap());
        }
    }
}
