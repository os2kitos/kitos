using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
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
using Core.DomainModel.Archive;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System.Collections.Generic;

namespace Infrastructure.DataAccess
{
    public class KitosContext : DbContext
    {
        private const string NpgsqlProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";
        private const string CaseInsensitiveTextColumnType = "citext";

        private static readonly HashSet<Type> CaseInsensitiveNameTypes = new HashSet<Type>
        {
            typeof(ItSystem),
            typeof(ItInterface),
            typeof(ItContract),
            typeof(Organization),
            typeof(OrganizationUnit),
            typeof(DataProcessingRegistration)
        };

        static KitosContext()
        {
            // Keep PostgreSQL timestamp handling compatible with existing non-UTC DateTime usage.
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        public KitosContext(DbContextOptions<KitosContext> options) : base(options) { }

        public DbSet<ItContractAgreementElementTypes> ItContractAgreementElementTypes { get; set; }
        public DbSet<OrganizationRight> OrganizationRights { get; set; }
        public DbSet<Core.DomainModel.Advice.Advice> Advices { get; set; }
        public DbSet<Core.DomainModel.Advice.AdviceUserRelation> AdviceUserRelations { get; set; }
        public DbSet<AgreementElementType> AgreementElementTypes { get; set; }
        public DbSet<ArchiveType> ArchiveTypes { get; set; }
        public DbSet<ArchiveLocation> ArchiveLocation { get; set; }
        public DbSet<ArchiveTestLocation> ArchiveTestLocation { get; set; }
        public DbSet<SystemUsageCriticalityLevel> SystemUsageCriticalityLevelTypes { get; set; }
        public DbSet<TechnicalSystemType> TechnicalSystemTypes { get; set; }
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
        public DbSet<OrganizationSupplier> OrganizationSuppliers { get; set; }
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
        public DbSet<ItSystemUsageArchive> ItSystemUsageArchives { get; set; }
        public DbSet<ArchiveReference> ArchiveReferences { get; set; }
        public DbSet<ItSystemUsageArchiveSnapshot> ItSystemUsageArchiveSnapshots { get; set; }
        public DbSet<LocalAgreementElementType> LocalAgreementElementTypes { get; set; }
        public DbSet<LocalArchiveType> LocalArchiveTypes { get; set; }
        public DbSet<LocalArchiveLocation> LocalArchiveLocation { get; set; }
        public DbSet<LocalArchiveTestLocation> LocalArchiveTestLocation { get; set; }
        public DbSet<LocalSystemUsageCriticalityLevel> LocalSystemUsageCriticalityLevelTypes { get; set; }
        public DbSet<LocalTechnicalSystemType> LocalTechnicalSystemTypes { get; set; }
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
        public DbSet<ItSystemUsageOverviewLocalTaskRefReadModel> ItSystemUsageOverviewLocalTaskRefReadModels { get; set; }
        public DbSet<ItSystemUsageOverviewSensitiveDataLevelReadModel> ItSystemUsageOverviewSensitiveDataLevelReadModels { get; set; }
        public DbSet<ItSystemUsageOverviewTechnicalSystemTypeReadModel> ItSystemUsageOverviewTechnicalSystemTypeReadModels { get; set; }
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
        public DbSet<ItContractSupplierOverviewReadModel> ItContractSupplierOverviewReadModels { get; set; }
        public DbSet<ItContractSupplierOverviewAtCriticalityContractReadModel> ItContractSupplierOverviewAtCriticalityContractReadModels { get; set; }
        public DbSet<StsOrganizationConnection> StsOrganizationConnections { get; set; }
        public DbSet<StsOrganizationChangeLog> StsOrganizationChangeLogs { get; set; }
        public DbSet<StsOrganizationConsequenceLog> StsOrganizationConsequenceLogs { get; set; }
        public DbSet<SubDataProcessor> SubDataProcessors { get; set; }
        public DbSet<ItSystemUsageOverviewRelevantOrgUnitReadModel> ItSystemUsageOverviewRelevantOrgUnitReadModels { get; set; }
        public DbSet<ItSystemUsageOverviewItContractReadModel> ItSystemUsageOverviewItContractReadModels { get; set; }
        public DbSet<CountryCode> CountryCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (IsNpgsqlProvider())
            {
                modelBuilder.UseIdentityByDefaultColumns();
                modelBuilder.HasDefaultSchema("dbo");
                modelBuilder.HasPostgresExtension(CaseInsensitiveTextColumnType);
            }

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(KitosContext).Assembly);

            ConfigureLocalOptionTypes(modelBuilder);

            if (IsNpgsqlProvider())
            {
                ConfigureCaseInsensitiveTextColumns(modelBuilder);
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var ignorePendingModelChangesWarning =
                string.Equals(Environment.GetEnvironmentVariable("IgnorePendingModelChangesWarning"), "true", StringComparison.OrdinalIgnoreCase);

            if (ignorePendingModelChangesWarning)
            {
                optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            }

            base.OnConfiguring(optionsBuilder);
        }

        private static void ConfigureLocalOptionTypes(ModelBuilder modelBuilder)
        {
            // Each LocalOptionEntity<T> concrete type maps to its own table.
            // EF Core cannot automatically resolve the polymorphic hierarchy of generic base types,
            // so each type is explicitly configured with its table name and with the obsolete
            // Option navigation ignored (it has no concrete CLR counterpart for EF Core to map).
            ConfigureLocalOptionType<LocalAgreementElementType>(modelBuilder, "LocalAgreementElementTypes");
            ConfigureLocalOptionType<LocalArchiveLocation>(modelBuilder, "LocalArchiveLocations");
            ConfigureLocalOptionType<LocalArchiveTestLocation>(modelBuilder, "LocalArchiveTestLocations");
            ConfigureLocalOptionType<LocalArchiveType>(modelBuilder, "LocalArchiveTypes");
            ConfigureLocalOptionType<LocalBusinessType>(modelBuilder, "LocalBusinessTypes");
            ConfigureLocalOptionType<LocalCriticalityType>(modelBuilder, "LocalCriticalityTypes");
            ConfigureLocalOptionType<LocalSystemUsageCriticalityLevel>(modelBuilder, "LocalSystemUsageCriticalityLevelTypes");
            ConfigureLocalOptionType<LocalTechnicalSystemType>(modelBuilder, "LocalTechnicalSystemTypes");
            ConfigureLocalOptionType<LocalDataProcessingBasisForTransferOption>(modelBuilder, "LocalDataProcessingBasisForTransferOptions");
            ConfigureLocalOptionType<LocalDataProcessingCountryOption>(modelBuilder, "LocalDataProcessingCountryOptions");
            ConfigureLocalOptionType<LocalDataProcessingDataResponsibleOption>(modelBuilder, "LocalDataProcessingDataResponsibleOptions");
            ConfigureLocalOptionType<LocalDataProcessingOversightOption>(modelBuilder, "LocalDataProcessingOversightOptions");
            ConfigureLocalOptionType<LocalDataProcessingRegistrationRole>(modelBuilder, "LocalDataProcessingRegistrationRoles");
            ConfigureLocalOptionType<LocalDataType>(modelBuilder, "LocalDataTypes");
            ConfigureLocalOptionType<LocalInterfaceType>(modelBuilder, "LocalInterfaceTypes");
            ConfigureLocalOptionType<LocalItContractRole>(modelBuilder, "LocalItContractRoles");
            ConfigureLocalOptionType<LocalItContractTemplateType>(modelBuilder, "LocalItContractTemplateTypes");
            ConfigureLocalOptionType<LocalItContractType>(modelBuilder, "LocalItContractTypes");
            ConfigureLocalOptionType<LocalItSystemCategories>(modelBuilder, "LocalItSystemCategories");
            ConfigureLocalOptionType<LocalItSystemRole>(modelBuilder, "LocalItSystemRoles");
            ConfigureLocalOptionType<LocalOptionExtendType>(modelBuilder, "LocalOptionExtendTypes");
            ConfigureLocalOptionType<LocalOrganizationUnitRole>(modelBuilder, "LocalOrganizationUnitRoles");
            ConfigureLocalOptionType<LocalPaymentFreqencyType>(modelBuilder, "LocalPaymentFreqencyTypes");
            ConfigureLocalOptionType<LocalPaymentModelType>(modelBuilder, "LocalPaymentModelTypes");
            ConfigureLocalOptionType<LocalPriceRegulationType>(modelBuilder, "LocalPriceRegulationTypes");
            ConfigureLocalOptionType<LocalProcurementStrategyType>(modelBuilder, "LocalProcurementStrategyTypes");
            ConfigureLocalOptionType<LocalPurchaseFormType>(modelBuilder, "LocalPurchaseFormTypes");
            ConfigureLocalOptionType<LocalRegisterType>(modelBuilder, "LocalRegisterTypes");
            ConfigureLocalOptionType<LocalRelationFrequencyType>(modelBuilder, "LocalRelationFrequencyTypes");
            ConfigureLocalOptionType<LocalSensitiveDataType>(modelBuilder, "LocalSensitiveDataTypes");
            ConfigureLocalOptionType<LocalSensitivePersonalDataType>(modelBuilder, "LocalSensitivePersonalDataTypes");
            ConfigureLocalOptionType<LocalTerminationDeadlineType>(modelBuilder, "LocalTerminationDeadlineTypes");
        }

        private static void ConfigureLocalOptionType<T>(ModelBuilder modelBuilder, string tableName)
            where T : class
        {
            modelBuilder.Entity<T>()
                .ToTable(tableName)
                .Ignore("Option");
        }

        private bool IsNpgsqlProvider()
        {
            return string.Equals(Database.ProviderName, NpgsqlProviderName, StringComparison.Ordinal);
        }

        private static void ConfigureCaseInsensitiveTextColumns(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                if (!ShouldApplyCaseInsensitiveNameConvention(entity))
                {
                    continue;
                }

                var nameProperty = entity.GetProperties()
                    .FirstOrDefault(property =>
                        property.ClrType == typeof(string)
                        && string.Equals(property.Name, nameof(IHasName.Name), StringComparison.Ordinal));

                nameProperty?.SetColumnType(CaseInsensitiveTextColumnType);
            }
        }

        private static bool ShouldApplyCaseInsensitiveNameConvention(IMutableEntityType entity)
        {
            var clrType = entity.ClrType;
            if (clrType == null)
            {
                return false;
            }

            if (CaseInsensitiveNameTypes.Contains(clrType))
            {
                return true;
            }

            return IsDerivedFromGenericType(clrType, typeof(OptionEntity<>));
        }

        private static bool IsDerivedFromGenericType(Type candidateType, Type genericBaseType)
        {
            for (var type = candidateType; type != null && type != typeof(object); type = type.BaseType)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericBaseType)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
