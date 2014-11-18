using System.Web.Http;
using System.Web.Security;
using Core.DomainServices;
using Core.ApplicationServices;
using Infrastructure.DataAccess;
using UI.MVC4.Infrastructure;
using UI.MVC4.Properties;

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
            
            kernel.Bind(typeof(IGenericRepository<>)).To(typeof(GenericRepository<>)).InRequestScope();
            kernel.Bind<IUserRepository>().To<UserRepository>().InRequestScope();
            kernel.Bind<IMailClient>().To<MailClient>().InRequestScope();
            kernel.Bind<ICryptoService>().To<CryptoService>();
            kernel.Bind<IUserService>().To<UserService>().InRequestScope()
                .WithConstructorArgument("ttl", Settings.Default.ResetPasswordTTL)
                .WithConstructorArgument("resetPasswordUrl", Settings.Default.ResetPasswordUrl);
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
