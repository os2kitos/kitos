using System;
using System.Web;
using System.Web.Security;
using Core.ApplicationServices;
using Core.DomainServices;
using Infrastructure.DataAccess;
using Infrastructure.OpenXML;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Extensions.Interception.Infrastructure.Language;
using Ninject.Web.Common;
using Presentation.Web;
using Presentation.Web.Infrastructure;
using Presentation.Web.Properties;
using Hangfire;
using Serilog;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(NinjectWebCommon), "Stop")]

namespace Presentation.Web
{
    public static class NinjectWebCommon
    {
        // ReSharper disable once InconsistentNaming
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

            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                //To enable ninject in hangfire
                GlobalConfiguration.Configuration.UseNinjectActivator(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
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
                .WithConstructorArgument("baseUrl", Settings.Default.BaseUrl)
                .WithConstructorArgument("mailSuffix", Settings.Default.MailSuffix);
            kernel.Bind<IOrgUnitService>().To<OrgUnitService>().InRequestScope();
            kernel.Bind<IOrganizationRoleService>().To<OrganizationRoleService>().InRequestScope();
            kernel.Bind<IAuthenticationService>().To<AuthenticationService>().InRequestScope();
            kernel.Bind<IAdviceService>().To<AdviceService>().InRequestScope();
            kernel.Bind<IOrganizationService>().To<OrganizationService>().InRequestScope();
            kernel.Bind<IItSystemService>().To<ItSystemService>().InRequestScope();
            kernel.Bind<IItProjectService>().To<ItProjectService>().InRequestScope();
            kernel.Bind<IItSystemUsageService>().To<ItSystemUsageService>().InRequestScope();
            // Udkommenteret ifm. OS2KITOS-663
            kernel.Bind<IItInterfaceService>().To<ItInterfaceService>().InRequestScope();
            kernel.Bind<IItContractService>().To<ItContractService>().InRequestScope();
            kernel.Bind<IUserRepositoryFactory>().To<UserRepositoryFactory>().InSingletonScope();
            kernel.Bind<IExcelService>().To<ExcelService>().InRequestScope();
            kernel.Bind<IExcelHandler>().To<ExcelHandler>().InRequestScope().Intercept().With(new LogInterceptor());
            kernel.Bind<IFeatureChecker>().To<FeatureChecker>().InSingletonScope();


            //MembershipProvider & Roleprovider injection - see ProviderInitializationHttpModule.cs
            kernel.Bind<MembershipProvider>().ToMethod(ctx => Membership.Provider);
            kernel.Bind<RoleProvider>().ToMethod(ctx => Roles.Provider);

            kernel.Bind<ILogger>().ToConstant(LogConfig.GlobalLogger).InTransientScope();

            // This however works, though the class is a bit ugly
            kernel.Bind<IHttpModule>().To<ProviderInitializationHttpModule>();
        }
    }
}
