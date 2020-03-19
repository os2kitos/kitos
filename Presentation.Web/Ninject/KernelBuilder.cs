using System;
using System.Collections.Generic;
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
using Core.ApplicationServices.Qa;
using Core.ApplicationServices.References;
using Core.ApplicationServices.SSO;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage;
using Core.ApplicationServices.SystemUsage.Migration;
using Core.ApplicationServices.TaskRefs;
using Core.BackgroundJobs.Factory;
using Core.BackgroundJobs.Model.ExternalLinks;
using Core.BackgroundJobs.Services;
using Core.DomainModel.Constants;
using Core.DomainModel.ItContract.DomainEvents;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DomainEvents;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.DomainEvents;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.References.DomainEvents;
using Core.DomainModel.Result;
using Core.DomainServices;
using Core.DomainServices.Context;
using Core.DomainServices.Model.EventHandlers;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.Interface;
using Core.DomainServices.Repositories.KLE;
using Core.DomainServices.Repositories.Project;
using Core.DomainServices.Repositories.Qa;
using Core.DomainServices.Repositories.Reference;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
using Core.DomainServices.SSO;
using Core.DomainServices.Time;
using Infrastructure.DataAccess;
using Infrastructure.DataAccess.Services;
using Infrastructure.OpenXML;
using Infrastructure.Services.BackgroundJobs;
using Infrastructure.Services.Configuration;
using Infrastructure.Services.Cryptography;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Http;
using Infrastructure.Services.KLEDataBridge;
using Microsoft.Owin;
using Ninject;
using Ninject.Extensions.Interception.Infrastructure.Language;
using Ninject.Web.Common;
using Presentation.Web.Infrastructure;
using Presentation.Web.Infrastructure.Factories.Authentication;
using Presentation.Web.Properties;
using Serilog;

namespace Presentation.Web.Ninject
{
    public class KernelBuilder
    {
        private readonly List<Action<StandardKernel>> _builderActions;

        private Maybe<KernelMode> _mode;

        private KernelMode Mode => _mode.Value;

        public KernelBuilder()
        {
            _builderActions = new List<Action<StandardKernel>>();
            _mode = Maybe<KernelMode>.None;
        }

        public KernelBuilder ForWebApplication()
        {
            if (_mode.HasValue)
            {
                throw new InvalidOperationException("Mode already set");
            }
            _mode = KernelMode.Web;

            _builderActions.Add(kernel => kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel));
            _builderActions.Add(kernel => kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>());

            return this;
        }

        public KernelBuilder ForHangFire()
        {
            if (_mode.HasValue)
            {
                throw new InvalidOperationException("Mode already set");
            }
            _mode = KernelMode.HangFireJob;

            return this;
        }

        public StandardKernel Build()
        {
            if (!_mode.HasValue)
            {
                throw new InvalidOperationException("Mode must be defined");
            }
            _builderActions.Add(RegisterServices);

            var standardKernel = new StandardKernel();
            _builderActions.ForEach(action => action(standardKernel));
            return standardKernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        public void RegisterServices(IKernel kernel)
        {
            RegisterDomainEventsEngine(kernel);
            RegisterDataAccess(kernel);
            kernel.Bind<KitosUrl>().ToMethod(_ => new KitosUrl(new Uri(Settings.Default.BaseUrl))).InSingletonScope();
            kernel.Bind<IMailClient>().To<MailClient>().InCommandScope(Mode);
            kernel.Bind<ICryptoService>().To<CryptoService>();
            kernel.Bind<IUserService>().To<UserService>().InCommandScope(Mode)
                .WithConstructorArgument("ttl", Settings.Default.ResetPasswordTTL)
                .WithConstructorArgument("baseUrl", Settings.Default.BaseUrl)
                .WithConstructorArgument("mailSuffix", Settings.Default.MailSuffix)
                .WithConstructorArgument("defaultUserPassword", Settings.Default.DefaultUserPassword)
                .WithConstructorArgument("useDefaultUserPassword", bool.Parse(Settings.Default.UseDefaultPassword));
            kernel.Bind<IOrgUnitService>().To<OrgUnitService>().InCommandScope(Mode);
            kernel.Bind<IOrganizationRoleService>().To<OrganizationRoleService>().InCommandScope(Mode);
            kernel.Bind<IOrganizationRightsService>().To<OrganizationRightsService>().InCommandScope(Mode);
            kernel.Bind<IAdviceService>().To<AdviceService>().InCommandScope(Mode);
            kernel.Bind<IOrganizationService>().To<OrganizationService>().InCommandScope(Mode);
            kernel.Bind<IItSystemService>().To<ItSystemService>().InCommandScope(Mode);
            kernel.Bind<IItProjectService>().To<ItProjectService>().InCommandScope(Mode);
            kernel.Bind<IItSystemUsageService>().To<ItSystemUsageService>().InCommandScope(Mode);
            kernel.Bind<IItInterfaceService>().To<ItInterfaceService>().InCommandScope(Mode);
            kernel.Bind<IItContractService>().To<ItContractService>().InCommandScope(Mode);
            kernel.Bind<IUserRepositoryFactory>().To<UserRepositoryFactory>().InSingletonScope();
            kernel.Bind<IExcelService>().To<ExcelService>().InCommandScope(Mode);
            kernel.Bind<IExcelHandler>().To<ExcelHandler>().InCommandScope(Mode).Intercept().With(new LogInterceptor());
            kernel.Bind<IItSystemUsageMigrationService>().To<ItSystemUsageMigrationService>().InCommandScope(Mode);
            kernel.Bind<IInterfaceExhibitUsageService>().To<InterfaceExhibitUsageService>().InCommandScope(Mode);
            kernel.Bind<IInterfaceUsageService>().To<InterfaceUsageService>().InCommandScope(Mode);
            kernel.Bind<IReferenceService>().To<ReferenceService>().InCommandScope(Mode);
            kernel.Bind<IEndpointValidationService>().To<EndpointValidationService>().InCommandScope(Mode);
            kernel.Bind<IBrokenExternalReferencesReportService>().To<BrokenExternalReferencesReportService>().InCommandScope(Mode);

            //MembershipProvider & Roleprovider injection - see ProviderInitializationHttpModule.cs
            kernel.Bind<MembershipProvider>().ToMethod(ctx => Membership.Provider);
            kernel.Bind<RoleProvider>().ToMethod(ctx => Roles.Provider);

            kernel.Bind<ILogger>().ToConstant(LogConfig.GlobalLogger).InTransientScope();
            kernel.Bind<IHttpModule>().To<ProviderInitializationHttpModule>();

            kernel.Bind<IOwinContext>().ToMethod(_ => HttpContext.Current.GetOwinContext()).InCommandScope(Mode);
            RegisterAuthenticationContext(kernel);
            RegisterAccessContext(kernel);
            RegisterKLE(kernel);
            RegisterOptions(kernel);
            RegisterBackgroundJobs(kernel);
            RegisterSSO(kernel);
        }

        private void RegisterSSO(IKernel kernel)
        {
            kernel.Bind<SsoFlowConfiguration>().ToMethod(_=>new SsoFlowConfiguration(Settings.Default.SsoServiceProviderId)).InSingletonScope();
            kernel.Bind<StsOrganisationIntegrationConfiguration>().ToMethod(_ =>
                new StsOrganisationIntegrationConfiguration(
                    Settings.Default.SsoCertificateThumbprint,
                    Settings.Default.StsOrganisationEndpointHost,
                    Settings.Default.StsOrganisationAuthorizedMunicipalityCvr))
                .InSingletonScope();

            kernel.Bind<ISsoFlowApplicationService>().To<SsoFlowApplicationService>().InCommandScope(Mode);
            kernel.Bind<IStsBrugerEmailService>().To<StsBrugerEmailService>().InCommandScope(Mode);
        }

        private void RegisterDomainEventsEngine(IKernel kernel)
        {
            kernel.Bind<IDomainEvents>().To<DomainEvents>().InCommandScope(Mode);
            RegisterDomainEvent<ExposingSystemChanged, RelationSpecificInterfaceEventsHandler>(kernel);
            RegisterDomainEvent<InterfaceDeleted, RelationSpecificInterfaceEventsHandler>(kernel);
            RegisterDomainEvent<ContractDeleted, ContractDeletedHandler>(kernel);
            RegisterDomainEvent<SystemUsageDeleted, SystemUsageDeletedHandler>(kernel);
            RegisterDomainEvent<InterfaceDeleted, UnbindBrokenReferenceReportsOnSourceDeletedHandler>(kernel);
            RegisterDomainEvent<ExternalReferenceDeleted, UnbindBrokenReferenceReportsOnSourceDeletedHandler>(kernel);

        }

        private void RegisterDomainEvent<TDomainEvent, THandler>(IKernel kernel)
            where TDomainEvent : IDomainEvent
            where THandler : IDomainEventHandler<TDomainEvent>
        {
            kernel.Bind<IDomainEventHandler<TDomainEvent>>().To<THandler>().InCommandScope(Mode);
        }

        private void RegisterOptions(IKernel kernel)
        {
            kernel.Bind<IOptionsService<SystemRelation, RelationFrequencyType>>()
                .To<OptionsService<SystemRelation, RelationFrequencyType, LocalRelationFrequencyType>>().InCommandScope(Mode);
        }

        private void RegisterKLE(IKernel kernel)
        {
            kernel.Bind<IKLEApplicationService>().To<KLEApplicationService>().InCommandScope(Mode);
            kernel.Bind<IKLEStandardRepository>().To<KLEStandardRepository>().InCommandScope(Mode);
            kernel.Bind<IKLEDataBridge>().To<KLEDataBridge>().InCommandScope(Mode);
            kernel.Bind<IKLEParentHelper>().To<KLEParentHelper>().InCommandScope(Mode);
            kernel.Bind<IKLEConverterHelper>().To<KLEConverterHelper>().InCommandScope(Mode);
            kernel.Bind<IKLEUpdateHistoryItemRepository>().To<KLEUpdateHistoryItemRepository>().InCommandScope(Mode);
            kernel.Bind<IOperationClock>().To<OperationClock>().InCommandScope(Mode);
        }

        private void RegisterDataAccess(IKernel kernel)
        {
            //Bind the kitos context
            kernel.Bind<KitosContext>().ToSelf().InCommandScope(Mode);

            kernel.Bind(typeof(IGenericRepository<>)).To(typeof(GenericRepository<>)).InCommandScope(Mode);
            kernel.Bind<IUserRepository>().To<UserRepository>().InCommandScope(Mode);
            kernel.Bind<IItSystemRepository>().To<ItSystemRepository>().InCommandScope(Mode);
            kernel.Bind<IItContractRepository>().To<ItContractRepository>().InCommandScope(Mode);
            kernel.Bind<ITransactionManager>().To<TransactionManager>().InCommandScope(Mode);
            kernel.Bind<IItSystemUsageRepository>().To<ItSystemUsageRepository>().InCommandScope(Mode);
            kernel.Bind<IItProjectRepository>().To<ItProjectRepository>().InCommandScope(Mode);
            kernel.Bind<IInterfaceRepository>().To<InterfaceRepository>().InCommandScope(Mode);
            kernel.Bind<IReferenceRepository>().To<ReferenceRepository>().InCommandScope(Mode);
            kernel.Bind<IBrokenExternalReferencesReportRepository>().To<BrokenExternalReferencesReportRepository>().InCommandScope(Mode);
            kernel.Bind<IEntityTypeResolver>().To<PocoTypeFromProxyResolver>().InCommandScope(Mode);
        }

        private void RegisterAuthenticationContext(IKernel kernel)
        {
            kernel.Bind<IAuthenticationContextFactory>().To<OwinAuthenticationContextFactory>().InCommandScope(Mode);
            kernel.Bind<IAuthenticationContext>().ToMethod(ctx => ctx.Kernel.Get<IAuthenticationContextFactory>().Create()).InCommandScope(Mode);
        }

        private void RegisterAccessContext(IKernel kernel)
        {
            //User context
            kernel.Bind<IUserContextFactory>().To<UserContextFactory>().InCommandScope(Mode);
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
                .InCommandScope(Mode);

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
            kernel.Bind<IAuthorizationContextFactory>().To<AuthorizationContextFactory>().InCommandScope(Mode);
            kernel.Bind<IAuthorizationContext>()
                .ToMethod(ctx =>
                {
                    var context = ctx.Kernel.Get<IOrganizationalUserContext>();
                    return ctx.Kernel.Get<IAuthorizationContextFactory>().Create(context);
                })
                .InCommandScope(Mode);
        }

        private void RegisterBackgroundJobs(IKernel kernel)
        {
            kernel.Bind<IBackgroundJobFactory>().To<BackgroundJobFactory>().InCommandScope(Mode);
            kernel.Bind<IBackgroundJobLauncher>().To<BackgroundJobLauncher>().InCommandScope(Mode);
            kernel.Bind<IBackgroundJobScheduler>().To<BackgroundJobScheduler>().InCommandScope(Mode);
            kernel.Bind<CheckExternalLinksBackgroundJob>().ToSelf().InCommandScope(Mode);
        }
    }
}