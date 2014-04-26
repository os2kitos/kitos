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
            kernel.Bind<IGenericRepository<Method>>().To<GenericRepository<Method>>().InRequestScope();
            kernel.Bind<IGenericRepository<Tsa>>().To<GenericRepository<Tsa>>().InRequestScope();
            kernel.Bind<IGenericRepository<Interface>>().To<GenericRepository<Interface>>().InRequestScope();
            kernel.Bind<IGenericRepository<DataType>>().To<GenericRepository<DataType>>().InRequestScope();
            kernel.Bind<IGenericRepository<PaymentModel>>().To<GenericRepository<PaymentModel>>().InRequestScope();
            kernel.Bind<IGenericRepository<ProjectPhase>>().To<GenericRepository<ProjectPhase>>().InRequestScope();
            kernel.Bind<IGenericRepository<ProjPhaseLocale>>().To<GenericRepository<ProjPhaseLocale>>().InRequestScope();
            kernel.Bind<IGenericRepository<ProjectCategory>>().To<GenericRepository<ProjectCategory>>().InRequestScope();
            kernel.Bind<IGenericRepository<ProjectType>>().To<GenericRepository<ProjectType>>().InRequestScope();
            kernel.Bind<IGenericRepository<PurchaseForm>>().To<GenericRepository<PurchaseForm>>().InRequestScope();
            kernel.Bind<IGenericRepository<AppType>>().To<GenericRepository<AppType>>().InRequestScope();
            kernel.Bind<IGenericRepository<BusinessType>>().To<GenericRepository<BusinessType>>().InRequestScope();
            kernel.Bind<IGenericRepository<HandoverTrial>>().To<GenericRepository<HandoverTrial>>().InRequestScope();
            kernel.Bind<IGenericRepository<AgreementElement>>().To<GenericRepository<AgreementElement>>().InRequestScope();
            kernel.Bind<IGenericRepository<ExtReferenceType>>().To<GenericRepository<ExtReferenceType>>().InRequestScope();
            kernel.Bind<IGenericRepository<ExtRefTypeLocale>>().To<GenericRepository<ExtRefTypeLocale>>().InRequestScope();
            kernel.Bind<IGenericRepository<TaskRef>>().To<GenericRepository<TaskRef>>().InRequestScope();
            kernel.Bind<IGenericRepository<TaskUsage>>().To<GenericRepository<TaskUsage>>().InRequestScope();
            kernel.Bind<IGenericRepository<ItSystem>>().To<GenericRepository<ItSystem>>().InRequestScope();

            kernel.Bind<IGenericRepository<ItProjectRole>>().To<GenericRepository<ItProjectRole>>().InRequestScope();
            kernel.Bind<IGenericRepository<ItContractRole>>().To<GenericRepository<ItContractRole>>().InRequestScope();
            kernel.Bind<IGenericRepository<ItSystemRole>>().To<GenericRepository<ItSystemRole>>().InRequestScope();
            kernel.Bind<IGenericRepository<OrganizationRole>>().To<GenericRepository<OrganizationRole>>().InRequestScope();

            kernel.Bind<IGenericRepository<OrganizationRight>>().To<GenericRepository<OrganizationRight>>().InRequestScope();

            kernel.Bind<IGenericRepository<AdminRight>>().To<GenericRepository<AdminRight>>().InRequestScope();
            kernel.Bind<IGenericRepository<AdminRole>>().To<GenericRepository<AdminRole>>().InRequestScope();

            kernel.Bind<IGenericRepository<Config>>().To<GenericRepository<Config>>();

            kernel.Bind<IGenericRepository<ItContractModuleName>>().To<GenericRepository<ItContractModuleName>>();
            kernel.Bind<IGenericRepository<ItProjectModuleName>>().To<GenericRepository<ItProjectModuleName>>();
            kernel.Bind<IGenericRepository<ItSupportModuleName>>().To<GenericRepository<ItSupportModuleName>>();
            kernel.Bind<IGenericRepository<ItSystemModuleName>>().To<GenericRepository<ItSystemModuleName>>();

            kernel.Bind<IUserRepository>().To<UserRepository>().InRequestScope();
            kernel.Bind<IMailClient>().To<MailClient>().InRequestScope();
            kernel.Bind<ICryptoService>().To<CryptoService>();
            kernel.Bind<IUserService>().To<UserService>().InRequestScope();
            kernel.Bind<IOrganizationService>().To<OrganizationService>().InRequestScope();
            kernel.Bind<IOrgUnitService>().To<OrgUnitService>().InRequestScope();
            kernel.Bind<IAdminService>().To<AdminService>().InRequestScope();
            kernel.Bind<IOrgService>().To<OrgService>().InRequestScope();
            kernel.Bind<IItSystemService>().To<ItSystemService>().InRequestScope();

            kernel.Bind<IUserRepositoryFactory>().To<UserRepositoryFactory>().InSingletonScope();

            //MembershipProvider & Roleprovider injection - see ProviderInitializationHttpModule.cs
            kernel.Bind<MembershipProvider>().ToMethod(ctx => Membership.Provider);
            kernel.Bind<RoleProvider>().ToMethod(ctx => Roles.Provider);
            kernel.Bind<IHttpModule>().To<ProviderInitializationHttpModule>();
        }
    }
}
