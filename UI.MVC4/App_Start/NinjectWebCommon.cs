using System.Web.Http;
using System.Web.Security;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainServices;
using Core.ApplicationServices;
using Infrastructure.DataAccess;
using UI.MVC4.Infrastructure;

[assembly: WebActivator.PreApplicationStartMethod(typeof(UI.MVC4.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(UI.MVC4.App_Start.NinjectWebCommon), "Stop")]

namespace UI.MVC4.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            RegisterServices(kernel);
            GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(kernel); // non API controllers
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind<KitosContext>().ToSelf().InRequestScope();

            kernel.Bind<IGenericRepository<Text>>().To<GenericRepository<Text>>().InRequestScope();
            kernel.Bind<IGenericRepository<Organization>>().To<GenericRepository<Organization>>().InRequestScope();
            kernel.Bind<IGenericRepository<OrganizationUnit>>().To<GenericRepository<OrganizationUnit>>().InRequestScope();
            kernel.Bind<IGenericRepository<User>>().To<GenericRepository<User>>().InRequestScope();
            kernel.Bind<IGenericRepository<PasswordResetRequest>>().To<GenericRepository<PasswordResetRequest>>().InRequestScope();
            kernel.Bind<IGenericRepository<ContractTemplate>>().To<GenericRepository<ContractTemplate>>().InRequestScope();
            kernel.Bind<IGenericRepository<ContractType>>().To<GenericRepository<ContractType>>().InRequestScope();
            kernel.Bind<IGenericRepository<InterfaceType>>().To<GenericRepository<InterfaceType>>().InRequestScope();
            kernel.Bind<IGenericRepository<InterfaceUsage>>().To<GenericRepository<InterfaceUsage>>().InRequestScope();
            kernel.Bind<IGenericRepository<InterfaceExposure>>().To<GenericRepository<InterfaceExposure>>().InRequestScope();
            kernel.Bind<IGenericRepository<Method>>().To<GenericRepository<Method>>().InRequestScope();
            kernel.Bind<IGenericRepository<Tsa>>().To<GenericRepository<Tsa>>().InRequestScope();
            kernel.Bind<IGenericRepository<Frequency>>().To<GenericRepository<Frequency>>().InRequestScope();
            kernel.Bind<IGenericRepository<Interface>>().To<GenericRepository<Interface>>().InRequestScope();
            kernel.Bind<IGenericRepository<DataType>>().To<GenericRepository<DataType>>().InRequestScope();
            kernel.Bind<IGenericRepository<DataRow>>().To<GenericRepository<DataRow>>().InRequestScope();
            kernel.Bind<IGenericRepository<DataRowUsage>>().To<GenericRepository<DataRowUsage>>().InRequestScope();
            kernel.Bind<IGenericRepository<ItProjectType>>().To<GenericRepository<ItProjectType>>().InRequestScope();
            kernel.Bind<IGenericRepository<PurchaseForm>>().To<GenericRepository<PurchaseForm>>().InRequestScope();
            kernel.Bind<IGenericRepository<AppType>>().To<GenericRepository<AppType>>().InRequestScope();
            kernel.Bind<IGenericRepository<BusinessType>>().To<GenericRepository<BusinessType>>().InRequestScope();
            kernel.Bind<IGenericRepository<AgreementElement>>().To<GenericRepository<AgreementElement>>().InRequestScope();
            kernel.Bind<IGenericRepository<ProcurementStrategy>>().To<GenericRepository<ProcurementStrategy>>().InRequestScope();
            kernel.Bind<IGenericRepository<TaskRef>>().To<GenericRepository<TaskRef>>().InRequestScope();
            kernel.Bind<IGenericRepository<TaskUsage>>().To<GenericRepository<TaskUsage>>().InRequestScope();
            kernel.Bind<IGenericRepository<ItSystem>>().To<GenericRepository<ItSystem>>().InRequestScope();
            kernel.Bind<IGenericRepository<ItSystemUsage>>().To<GenericRepository<ItSystemUsage>>().InRequestScope();
            kernel.Bind<IGenericRepository<ItProjectRole>>().To<GenericRepository<ItProjectRole>>().InRequestScope();
            kernel.Bind<IGenericRepository<ItContract>>().To<GenericRepository<ItContract>>().InRequestScope();
            kernel.Bind<IGenericRepository<ItContractRole>>().To<GenericRepository<ItContractRole>>().InRequestScope();
            kernel.Bind<IGenericRepository<ItSystemRole>>().To<GenericRepository<ItSystemRole>>().InRequestScope();
            kernel.Bind<IGenericRepository<OrganizationRole>>().To<GenericRepository<OrganizationRole>>().InRequestScope();
            kernel.Bind<IGenericRepository<ArchiveType>>().To<GenericRepository<ArchiveType>>().InRequestScope();
            kernel.Bind<IGenericRepository<SensitiveDataType>>().To<GenericRepository<SensitiveDataType>>().InRequestScope();
            kernel.Bind<IGenericRepository<Wish>>().To<GenericRepository<Wish>>().InRequestScope();
            kernel.Bind<IGenericRepository<ItProject>>().To<GenericRepository<ItProject>>().InRequestScope();
            kernel.Bind<IGenericRepository<Risk>>().To<GenericRepository<Risk>>().InRequestScope();
            kernel.Bind<IGenericRepository<Activity>>().To<GenericRepository<Activity>>().InRequestScope();
            kernel.Bind<IGenericRepository<State>>().To<GenericRepository<State>>().InRequestScope();
            kernel.Bind<IGenericRepository<GoalStatus>>().To<GenericRepository<GoalStatus>>().InRequestScope();
            kernel.Bind<IGenericRepository<Goal>>().To<GenericRepository<Goal>>().InRequestScope();
            kernel.Bind<IGenericRepository<GoalType>>().To<GenericRepository<GoalType>>().InRequestScope();
            kernel.Bind<IGenericRepository<EconomyYear>>().To<GenericRepository<EconomyYear>>().InRequestScope();
            kernel.Bind<IGenericRepository<Communication>>().To<GenericRepository<Communication>>().InRequestScope();
            kernel.Bind<IGenericRepository<Handover>>().To<GenericRepository<Handover>>().InRequestScope();
            kernel.Bind<IGenericRepository<Stakeholder>>().To<GenericRepository<Stakeholder>>().InRequestScope();
            kernel.Bind<IGenericRepository<OptionExtend>>().To<GenericRepository<OptionExtend>>().InRequestScope();
            kernel.Bind<IGenericRepository<PriceRegulation>>().To<GenericRepository<PriceRegulation>>().InRequestScope();
            kernel.Bind<IGenericRepository<PaymentModel>>().To<GenericRepository<PaymentModel>>().InRequestScope();
            kernel.Bind<IGenericRepository<PaymentFreqency>>().To<GenericRepository<PaymentFreqency>>().InRequestScope();
            kernel.Bind<IGenericRepository<TerminationDeadline>>().To<GenericRepository<TerminationDeadline>>().InRequestScope();
            kernel.Bind<IGenericRepository<PaymentMilestone>>().To<GenericRepository<PaymentMilestone>>().InRequestScope();
            kernel.Bind<IGenericRepository<EconomyStream>>().To<GenericRepository<EconomyStream>>().InRequestScope();
            kernel.Bind<IGenericRepository<Advice>>().To<GenericRepository<Advice>>().InRequestScope();
            kernel.Bind<IGenericRepository<OrganizationRight>>().To<GenericRepository<OrganizationRight>>().InRequestScope();
            kernel.Bind<IGenericRepository<ItSystemRight>>().To<GenericRepository<ItSystemRight>>().InRequestScope();
            kernel.Bind<IGenericRepository<ItProjectRight>>().To<GenericRepository<ItProjectRight>>().InRequestScope();
            kernel.Bind<IGenericRepository<ItContractRight>>().To<GenericRepository<ItContractRight>>().InRequestScope();
            kernel.Bind<IGenericRepository<AdminRight>>().To<GenericRepository<AdminRight>>().InRequestScope();
            kernel.Bind<IGenericRepository<AdminRole>>().To<GenericRepository<AdminRole>>().InRequestScope();
            kernel.Bind<IGenericRepository<Config>>().To<GenericRepository<Config>>();
            kernel.Bind<IUserRepository>().To<UserRepository>().InRequestScope();
            kernel.Bind<IMailClient>().To<MailClient>().InRequestScope();
            kernel.Bind<ICryptoService>().To<CryptoService>();
            kernel.Bind<IUserService>().To<UserService>().InRequestScope();
            kernel.Bind<IOrgUnitService>().To<OrgUnitService>().InRequestScope();
            kernel.Bind<IAdminService>().To<AdminService>().InRequestScope();
            kernel.Bind<IOrganizationService>().To<OrganizationService>().InRequestScope();
            kernel.Bind<IItSystemService>().To<ItSystemService>().InRequestScope();
            kernel.Bind<IItProjectService>().To<ItProjectService>().InRequestScope();
            kernel.Bind<IItSystemUsageService>().To<ItSystemUsageService>().InRequestScope();
            kernel.Bind<IUserRepositoryFactory>().To<UserRepositoryFactory>().InSingletonScope();

            //MembershipProvider & Roleprovider injection - see ProviderInitializationHttpModule.cs
            kernel.Bind<MembershipProvider>().ToMethod(ctx => Membership.Provider);
            kernel.Bind<RoleProvider>().ToMethod(ctx => Roles.Provider);
            kernel.Bind<IHttpModule>().To<ProviderInitializationHttpModule>();
        }
    }
}
