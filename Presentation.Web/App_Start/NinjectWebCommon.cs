using System;
using System.Web;
using System.Web.Security;
using Core.ApplicationServices;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Interface.ExhibitUsage;
using Core.ApplicationServices.Interface.Usage;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Project;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage;
using Core.ApplicationServices.SystemUsage.Migration;
using Core.DomainServices;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.Project;
using Core.DomainServices.Repositories.Reference;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
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
using Infrastructure.DataAccess.Services;
using Infrastructure.Services.Cryptography;
using Infrastructure.Services.DataAccess;
using Microsoft.Owin;
using Presentation.Web.Infrastructure.Factories.Authentication;
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
            RegisterDataAccess(kernel);

            kernel.Bind<IMailClient>().To<MailClient>().InRequestScope();
            kernel.Bind<ICryptoService>().To<CryptoService>();
            kernel.Bind<IUserService>().To<UserService>().InRequestScope()
                .WithConstructorArgument("ttl", Settings.Default.ResetPasswordTTL)
                .WithConstructorArgument("baseUrl", Settings.Default.BaseUrl)
                .WithConstructorArgument("mailSuffix", Settings.Default.MailSuffix)
                .WithConstructorArgument("defaultUserPassword", Settings.Default.DefaultUserPassword)
                .WithConstructorArgument("useDefaultUserPassword", bool.Parse(Settings.Default.UseDefaultPassword));
            kernel.Bind<IOrgUnitService>().To<OrgUnitService>().InRequestScope();
            kernel.Bind<IOrganizationRoleService>().To<OrganizationRoleService>().InRequestScope();
            kernel.Bind<IOrganizationRightsService>().To<OrganizationRightsService>().InRequestScope();
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
            kernel.Bind<IFeatureChecker>().To<FeatureChecker>().InRequestScope();
            kernel.Bind<IItSystemUsageMigrationService>().To<ItSystemUsageMigrationService>().InRequestScope();
            kernel.Bind<IInterfaceExhibitUsageService>().To<InterfaceExhibitUsageService>().InRequestScope();
            kernel.Bind<IInterfaceUsageService>().To<InterfaceUsageService>().InRequestScope();
            kernel.Bind<IReferenceService>().To<ReferenceService>().InRequestScope();

            //MembershipProvider & Roleprovider injection - see ProviderInitializationHttpModule.cs
            kernel.Bind<MembershipProvider>().ToMethod(ctx => Membership.Provider);
            kernel.Bind<RoleProvider>().ToMethod(ctx => Roles.Provider);

            kernel.Bind<ILogger>().ToConstant(LogConfig.GlobalLogger).InTransientScope();
            kernel.Bind<IHttpModule>().To<ProviderInitializationHttpModule>();

            kernel.Bind<IOwinContext>().ToMethod(_ => HttpContext.Current.GetOwinContext()).InRequestScope();
            RegisterAuthenticationContext(kernel);
            RegisterAccessContext(kernel);
        }

        private static void RegisterDataAccess(IKernel kernel)
        {
            kernel.Bind<KitosContext>().ToSelf().InRequestScope();
            kernel.Bind(typeof(IGenericRepository<>)).To(typeof(GenericRepository<>)).InRequestScope();
            kernel.Bind<IUserRepository>().To<UserRepository>().InRequestScope();
            kernel.Bind<IItSystemRepository>().To<ItSystemRepository>().InRequestScope();
            kernel.Bind<IItContractRepository>().To<ItContractRepository>().InRequestScope();
            kernel.Bind<ITransactionManager>().To<TransactionManager>().InRequestScope();
            kernel.Bind<IItSystemUsageRepository>().To<ItSystemUsageRepository>().InRequestScope();
            kernel.Bind<IItProjectRepository>().To<ItProjectRepository>().InRequestScope();
            kernel.Bind<IReferenceRepository>().To<ReferenceRepository>().InRequestScope();
        }

        private static void RegisterAuthenticationContext(IKernel kernel)
        {
            kernel.Bind<IAuthenticationContextFactory>().To<OwinAuthenticationContextFactory>().InRequestScope();
            kernel.Bind<IAuthenticationContext>().ToMethod(ctx => ctx.Kernel.Get<IAuthenticationContextFactory>().Create())
                .InRequestScope();
        }

        private static void RegisterAccessContext(IKernel kernel)
        {
            //User context
            kernel.Bind<IUserContextFactory>().To<UserContextFactory>().InRequestScope();
            kernel.Bind<IOrganizationalUserContext>()
                .ToMethod(ctx =>
                {
                    var factory = ctx.Kernel.Get<IUserContextFactory>();
                    var authentication = ctx.Kernel.Get<IAuthenticationContext>();
                    bool canCreateContext = authentication.Method != AuthenticationMethod.Anonymous && authentication.ActiveOrganizationId.HasValue;

                    if (canCreateContext)
                    {
                        return factory.Create(authentication.UserId.GetValueOrDefault(), authentication.ActiveOrganizationId.GetValueOrDefault());
                    }

                    return new UnauthenticatedUserContext();
                })
                .InRequestScope();

            //Authorization context
            kernel.Bind<IAuthorizationContextFactory>().To<AuthorizationContextFactory>().InRequestScope();
            kernel.Bind<IAuthorizationContext>()
                .ToMethod(ctx =>
                {
                    var context = ctx.Kernel.Get<IOrganizationalUserContext>();
                    return ctx.Kernel.Get<IAuthorizationContextFactory>().Create(context);
                })
                .InRequestScope();
        }
    }
}
