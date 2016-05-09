using System.Web.Security;
using Core.ApplicationServices;
using Core.DomainServices;
using Infrastructure.DataAccess;
using Infrastructure.OpenXML;
using Ninject.Extensions.Interception.Infrastructure.Language;
using Ninject.Extensions.Logging;
using Ninject.Extensions.Logging.Serilog;
using Ninject.Extensions.Logging.Serilog.Infrastructure;
using Ninject.Modules;
using Ninject.Web.WebApi;
using Ninject.Web.WebApi.WebHost;
using Presentation.Web.Infrastructure;
using Presentation.Web.Properties;
using Serilog;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Presentation.Web.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Presentation.Web.App_Start.NinjectWebCommon), "Stop")]

namespace Presentation.Web.App_Start
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

            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
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
            kernel.Bind<IUserRepository>().To<UserRepository>().InRequestScope().Intercept().With<LogInterceptor>();
            kernel.Bind<IMailClient>().To<MailClient>().InRequestScope().Intercept().With<LogInterceptor>();
            kernel.Bind<ICryptoService>().To<CryptoService>().Intercept().With<LogInterceptor>();
            kernel.Bind<IUserService>().To<UserService>().InRequestScope()
                .WithConstructorArgument("ttl", Settings.Default.ResetPasswordTTL)
                .WithConstructorArgument("baseUrl", Settings.Default.BaseUrl)
                .WithConstructorArgument("mailSuffix", Settings.Default.MailSuffix).Intercept().With<LogInterceptor>();
            kernel.Bind<IOrgUnitService>().To<OrgUnitService>().InRequestScope().Intercept().With<LogInterceptor>();
            kernel.Bind<IAdminService>().To<AdminService>().InRequestScope().Intercept().With<LogInterceptor>();
            kernel.Bind<IOrganizationService>().To<OrganizationService>().InRequestScope().Intercept().With<LogInterceptor>();
            kernel.Bind<IItSystemService>().To<ItSystemService>().InRequestScope().Intercept().With<LogInterceptor>();
            kernel.Bind<IItProjectService>().To<ItProjectService>().InRequestScope().Intercept().With<LogInterceptor>();
            kernel.Bind<IItSystemUsageService>().To<ItSystemUsageService>().InRequestScope().Intercept().With<LogInterceptor>();
            kernel.Bind<IUserRepositoryFactory>().To<UserRepositoryFactory>().InSingletonScope().Intercept().With<LogInterceptor>();
            kernel.Bind<IExcelService>().To<ExcelService>().InRequestScope().Intercept().With<LogInterceptor>();
            kernel.Bind<IExcelHandler>().To<ExcelHandler>().InRequestScope().Intercept().With<LogInterceptor>();

            //MembershipProvider & Roleprovider injection - see ProviderInitializationHttpModule.cs
            kernel.Bind<MembershipProvider>().ToMethod(ctx => Membership.Provider).Intercept().With<LogInterceptor>();
            kernel.Bind<RoleProvider>().ToMethod(ctx => Roles.Provider).Intercept().With<LogInterceptor>();
            kernel.Bind<IHttpModule>().To<ProviderInitializationHttpModule>();
        }
    }
}
