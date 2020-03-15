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
using Core.ApplicationServices.KLE;
using Core.ApplicationServices.Options;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Project;
using Core.ApplicationServices.References;
using Core.ApplicationServices.SSO;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage;
using Core.ApplicationServices.SystemUsage.Migration;
using Core.ApplicationServices.TaskRefs;
using Core.DomainModel.Constants;
using Core.DomainModel.ItContract.DomainEvents;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DomainEvents;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.DomainEvents;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Context;
using Core.DomainServices.Model.EventHandlers;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.KLE;
using Core.DomainServices.Repositories.Project;
using Core.DomainServices.Repositories.Reference;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
using Core.DomainServices.Time;
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
using Infrastructure.Services.KLEDataBridge;
using Microsoft.Owin;
using Presentation.Web.Infrastructure.Factories.Authentication;
using Serilog;
using Infrastructure.Services.DomainEvents;

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
            RegisterDomainEventsEngine(kernel);
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
            kernel.Bind<IItInterfaceService>().To<ItInterfaceService>().InRequestScope();
            kernel.Bind<IItContractService>().To<ItContractService>().InRequestScope();
            kernel.Bind<IUserRepositoryFactory>().To<UserRepositoryFactory>().InSingletonScope();
            kernel.Bind<IExcelService>().To<ExcelService>().InRequestScope();
            kernel.Bind<IExcelHandler>().To<ExcelHandler>().InRequestScope().Intercept().With(new LogInterceptor());
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
            RegisterKLE(kernel);
            RegisterSSO(kernel);
            RegisterOptions(kernel);
        }

        private static void RegisterDomainEventsEngine(IKernel kernel)
        {
            kernel.Bind<IDomainEvents>().To<DomainEvents>().InRequestScope();
            kernel.Bind<IDomainEventHandler<ExposingSystemChanged>>().To<RelationSpecificInterfaceEventsHandler>().InRequestScope();
            kernel.Bind<IDomainEventHandler<InterfaceDeleted>>().To<RelationSpecificInterfaceEventsHandler>().InRequestScope();
            kernel.Bind<IDomainEventHandler<ContractDeleted>>().To<ContractDeletedHandler>().InRequestScope();
            kernel.Bind<IDomainEventHandler<SystemUsageDeleted>>().To<SystemUsageDeletedHandler>().InRequestScope();
        }

        private static void RegisterOptions(IKernel kernel)
        {
            kernel.Bind<IOptionsService<SystemRelation, RelationFrequencyType>>()
                .To<OptionsService<SystemRelation, RelationFrequencyType, LocalRelationFrequencyType>>().InRequestScope();
        }

        private static void RegisterKLE(IKernel kernel)
        {
            kernel.Bind<IKLEApplicationService>().To<KLEApplicationService>().InRequestScope();
            kernel.Bind<IKLEStandardRepository>().To<KLEStandardRepository>().InRequestScope();
            kernel.Bind<IKLEDataBridge>().To<KLEDataBridge>().InRequestScope();
            kernel.Bind<IKLEParentHelper>().To<KLEParentHelper>().InRequestScope();
            kernel.Bind<IKLEConverterHelper>().To<KLEConverterHelper>().InRequestScope();
            kernel.Bind<IKLEUpdateHistoryItemRepository>().To<KLEUpdateHistoryItemRepository>().InRequestScope();
            kernel.Bind<IOperationClock>().To<OperationClock>().InRequestScope();
        }

        private static void RegisterSSO(IKernel kernel)
        {
            kernel.Bind<ISSOFlowApplicationService>().To<SSOFlowApplicationService>().InRequestScope();
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
            kernel.Bind<IEntityTypeResolver>().To<PocoTypeFromProxyResolver>().InRequestScope();
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
                    var canCreateContext = authentication.Method != AuthenticationMethod.Anonymous;

                    if (canCreateContext)
                    {
                        return factory.Create(authentication.UserId.GetValueOrDefault(), authentication.ActiveOrganizationId.GetValueOrDefault(EntityConstants.InvalidActiveOrganizationId));
                    }

                    return new UnauthenticatedUserContext();
                })
                .InRequestScope();

            //Injecting it as a maybe since service calls and background processes do not have an active user
            kernel.Bind<Maybe<ActiveUserContext>>()
                .ToMethod(ctx =>
                {
                    var authentication = ctx.Kernel.Get<IAuthenticationContext>();
                    if (authentication.Method == AuthenticationMethod.Anonymous)
                    {
                        return Maybe<ActiveUserContext>.None;
                    }

                    var userContext = ctx.Kernel.Get<IOrganizationalUserContext>();
                    return new ActiveUserContext(userContext.ActiveOrganizationId, userContext.UserEntity);
                });

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
