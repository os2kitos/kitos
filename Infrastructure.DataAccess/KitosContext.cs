using System.Data.Entity;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Infrastructure.DataAccess.Mapping;
using MySql.Data.Entity;

namespace Infrastructure.DataAccess
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class KitosContext : DbContext
    {
        static KitosContext()
        {

        }

        public KitosContext()
            : base("Name=KitosContext")
        {
            Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }

        public DbSet<OrganizationRight> OrganizationRights { get; set; }
        public DbSet<OrganizationRole> OrganizationRoles { get; set; }
        public DbSet<Advice> Advices { get; set; }
        public DbSet<AgreementElementType> AgreementElementTypes { get; set; }
        public DbSet<ArchiveType> ArchiveTypes { get; set; }
        public DbSet<BusinessType> BusinessTypes { get; set; }
        public DbSet<Communication> Communications { get; set; }
        public DbSet<Config> Configs { get; set; }
        public DbSet<ContractTemplateType> ContractTemplateTypes { get; set; }
        public DbSet<ContractType> ContractTypes { get; set; }
        public DbSet<DataType> DataTypes { get; set; }
        public DbSet<DataRow> DataRows { get; set; }
        public DbSet<DataRowUsage> DataRowUsages { get; set; }
        public DbSet<EconomyYear> EconomyYears { get; set; }
        public DbSet<EconomyStream> EconomyStrams { get; set; }
        public DbSet<FrequencyType> FrequencyTypes { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<GoalStatus> GoalStatus { get; set; }
        public DbSet<GoalType> GoalTypes { get; set; }
        public DbSet<Handover> Handovers { get; set; }
        public DbSet<HandoverTrial> HandoverTrials { get; set; }
        public DbSet<HandoverTrialType> HandoverTrialTypes { get; set; }
        public DbSet<Interface> Interfaces { get; set; }
        public DbSet<ItInterfaceUse> ItInterfaceUses { get; set; }
        public DbSet<ItInterfaceUsage> ItInterfaceUsages { get; set; }
        public DbSet<ItInterfaceExhibit> ItInterfaceExhibits { get; set; }
        public DbSet<ItInterfaceExhibitUsage> ItInterfaceExhibtUsages { get; set; }
        public DbSet<InterfaceType> InterfaceTypes { get; set; }
        public DbSet<ItContract> ItContracts { get; set; }
        public DbSet<ItContractItSystemUsage> ItContractItSystemUsages { get; set; }
        public DbSet<ItContractRight> ItContractRights { get; set; }
        public DbSet<ItContractRole> ItContractRoles { get; set; }
        public DbSet<ItProject> ItProjects { get; set; }
        public DbSet<ItProjectStatus> ItProjectStatuses { get; set; }
        public DbSet<ItProjectRight> ItProjectRights { get; set; }
        public DbSet<ItProjectRole> ItProjectRoles { get; set; }
        public DbSet<ItProjectOrgUnitUsage> ItProjectOrgUnitUsages { get; set; }
        public DbSet<ItSystemUsageOrgUnitUsage> ItSystemUsageOrgUnitUsages { get; set; }
        public DbSet<ItSystem> ItSystems { get; set; }
        public DbSet<ItSystemUsage> ItSystemUsages { get; set; }
        public DbSet<ItSystemRight> ItSystemRights { get; set; }
        public DbSet<ItSystemRole> ItSystemRoles { get; set; }
        public DbSet<ItSystemType> ItSystemTypes { get; set; }
        public DbSet<MethodType> MethodTypes { get; set; }
        public DbSet<OptionExtendType> OptionExtendTypes { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
        public DbSet<OrganizationUnitRight> OrganizationUnitRights { get; set; }
        public DbSet<OrganizationUnitRole> OrganizationUnitRoles { get; set; }
        public DbSet<PasswordResetRequest> PasswordResetRequests { get; set; }
        public DbSet<PaymentFreqencyType> PaymentFreqencyTypes { get; set; }
        public DbSet<PaymentMilestone> PaymentMilestones { get; set; }
        public DbSet<PaymentModelType> PaymentModelTypes { get; set; }
        public DbSet<PriceRegulationType> PriceRegulationTypes { get; set; }
        public DbSet<ProcurementStrategyType> ProcurementStrategyTypes { get; set; }
        public DbSet<ItProjectType> ProjectTypes { get; set; }
        public DbSet<PurchaseFormType> PurchaseFormTypes { get; set; }
        public DbSet<Risk> Risks { get; set; }
        public DbSet<SensitiveDataType> SensitiveDataTypes { get; set; }
        public DbSet<Stakeholder> Stakeholders { get; set; }
        public DbSet<TerminationDeadlineType> TerminationDeadlineTypes { get; set; }
        public DbSet<TaskRef> TaskRefs { get; set; }
        public DbSet<TaskUsage> TaskUsages { get; set; }
        public DbSet<Text> Texts { get; set; }
        public DbSet<TsaType> TsaTypes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Wish> Wishes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.ComplexType<ItProjectPhase>();

            modelBuilder.Configurations.Add(new OrganizationRightMap());
            modelBuilder.Configurations.Add(new OrganizationRoleMap());
            modelBuilder.Configurations.Add(new AdviceMap());
            modelBuilder.Configurations.Add(new AgreementElementMap());
            modelBuilder.Configurations.Add(new ArchiveTypeMap());
            modelBuilder.Configurations.Add(new BusinessTypeMap());
            modelBuilder.Configurations.Add(new CommunicationMap());
            modelBuilder.Configurations.Add(new ConfigMap());
            modelBuilder.Configurations.Add(new ContractTemplateMap());
            modelBuilder.Configurations.Add(new ContractTypeMap());
            modelBuilder.Configurations.Add(new DataTypeMap());
            modelBuilder.Configurations.Add(new DataRowMap());
            modelBuilder.Configurations.Add(new DataRowUsageMap());
            modelBuilder.Configurations.Add(new EconomyStreamMap());
            modelBuilder.Configurations.Add(new EconomyYearMap());
            modelBuilder.Configurations.Add(new FrequencyTypeMap());
            modelBuilder.Configurations.Add(new GoalMap());
            modelBuilder.Configurations.Add(new GoalStatusMap());
            modelBuilder.Configurations.Add(new GoalTypeMap());
            modelBuilder.Configurations.Add(new HandoverMap());
            modelBuilder.Configurations.Add(new HandoverTrialMap());
            modelBuilder.Configurations.Add(new HandoverTrialTypeMap());
            modelBuilder.Configurations.Add(new InterfaceMap());
            modelBuilder.Configurations.Add(new ItInterfaceUsageMap());
            modelBuilder.Configurations.Add(new ItInterfaceMap());
            modelBuilder.Configurations.Add(new ItInterfaceUseMap());
            modelBuilder.Configurations.Add(new ItInterfaceExhibitMap());
            modelBuilder.Configurations.Add(new ItInterfaceExhibitUsageMap());
            modelBuilder.Configurations.Add(new InterfaceTypeMap());
            modelBuilder.Configurations.Add(new ItContractMap());
            modelBuilder.Configurations.Add(new ItContractItSystemUsageMap());
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
            modelBuilder.Configurations.Add(new ItSystemTypeMap());
            modelBuilder.Configurations.Add(new MethodTypeMap());
            modelBuilder.Configurations.Add(new OrganizationMap());
            modelBuilder.Configurations.Add(new OrganizationUnitMap());
            modelBuilder.Configurations.Add(new OrganizationUnitRightMap());
            modelBuilder.Configurations.Add(new OrganizationUnitRoleMap());
            modelBuilder.Configurations.Add(new PasswordResetRequestMap());
            modelBuilder.Configurations.Add(new ItProjectTypeMap());
            modelBuilder.Configurations.Add(new ProcurementStrategyTypeMap());
            modelBuilder.Configurations.Add(new PurchaseFormTypeMap());
            modelBuilder.Configurations.Add(new RiskMap());
            modelBuilder.Configurations.Add(new SensitiveDataTypeMap());
            modelBuilder.Configurations.Add(new StakeholderMap());
            modelBuilder.Configurations.Add(new TaskRefMap());
            modelBuilder.Configurations.Add(new TaskUsageMap());
            modelBuilder.Configurations.Add(new TextMap());
            modelBuilder.Configurations.Add(new TsaTypeMap());
            modelBuilder.Configurations.Add(new TerminationDeadlineTypeMap());
            modelBuilder.Configurations.Add(new UserMap());
            modelBuilder.Configurations.Add(new WishMap());
            modelBuilder.Configurations.Add(new PriceRegulationTypeMap());
            modelBuilder.Configurations.Add(new PaymentModelTypeMap());
            modelBuilder.Configurations.Add(new PaymentFreqencyTypeMap());
            modelBuilder.Configurations.Add(new OptionExtendTypeMap());
        }
    }
}
