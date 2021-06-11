using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using AutoMapper;
using Core.ApplicationServices;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.KLE;
using Core.ApplicationServices.Model.EventHandler;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Project;
using Core.ApplicationServices.Qa;
using Core.ApplicationServices.References;
using Core.ApplicationServices.ScheduledJobs;
using Core.ApplicationServices.SSO;
using Core.ApplicationServices.SSO.Factories;
using Core.ApplicationServices.System;
using Core.ApplicationServices.SystemUsage;
using Core.ApplicationServices.SystemUsage.GDPR;
using Core.ApplicationServices.SystemUsage.Migration;
using Core.BackgroundJobs.Factories;
using Core.BackgroundJobs.Model.ExternalLinks;
using Core.BackgroundJobs.Model.ReadModels;
using Core.BackgroundJobs.Services;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.ItContract.DomainEvents;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystem.DomainEvents;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Organization;
using Core.DomainModel.Organization.DomainEvents;
using Core.DomainServices;
using Core.DomainServices.Context;
using Core.DomainServices.GDPR;
using Core.DomainServices.Model;
using Core.DomainServices.Model.EventHandlers;
using Core.DomainServices.Repositories.Advice;
using Core.DomainServices.Options;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.Interface;
using Core.DomainServices.Repositories.KLE;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.Project;
using Core.DomainServices.Repositories.Qa;
using Core.DomainServices.Repositories.Reference;
using Core.DomainServices.Repositories.SSO;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
using Core.DomainServices.SSO;
using Core.DomainServices.Time;
using dk.nita.saml20.identity;
using Infrastructure.DataAccess;
using Infrastructure.DataAccess.Services;
using Infrastructure.OpenXML;
using Infrastructure.Services.BackgroundJobs;
using Infrastructure.Services.Caching;
using Infrastructure.Services.Configuration;
using Infrastructure.Services.Cryptography;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.DomainEvents;
using Infrastructure.Services.Http;
using Infrastructure.Services.KLEDataBridge;
using Infrastructure.Services.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Owin;
using Ninject;
using Ninject.Extensions.Interception.Infrastructure.Language;
using Ninject.Web.Common;
using Presentation.Web.Infrastructure;
using Presentation.Web.Infrastructure.Factories.Authentication;
using Presentation.Web.Properties;
using Serilog;
using Core.DomainServices.Contract;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainServices.SystemUsage;
using Core.DomainModel.ItProject;
using Core.DomainServices.Advice;
using Core.DomainServices.Repositories.Kendo;
using Core.ApplicationServices.Notification;
using Core.ApplicationServices.RightsHolders;
using Core.DomainServices.Repositories.Notification;
using Core.DomainServices.Notifications;
using Core.ApplicationServices.OptionTypes;

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

            //Register automapper
            _builderActions.Add(kernel => kernel.Bind<IMapper>().ToConstant(MappingConfig.CreateMapper()).InSingletonScope());

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
            kernel.Bind<IObjectCache>().To<AspNetObjectCache>().InSingletonScope();
            kernel.Bind<KitosUrl>().ToMethod(_ => new KitosUrl(new Uri(Settings.Default.BaseUrl))).InSingletonScope();
            kernel.Bind<IMailClient>().To<SingleThreadedMailClient>().InCommandScope(Mode);
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
            kernel.Bind<AdviceService>().ToSelf().InCommandScope(Mode);
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
            kernel.Bind<IReferenceService>().To<ReferenceService>().InCommandScope(Mode);
            kernel.Bind<IEndpointValidationService>().To<EndpointValidationService>().InCommandScope(Mode);
            kernel.Bind<IBrokenExternalReferencesReportService>().To<BrokenExternalReferencesReportService>().InCommandScope(Mode);
            kernel.Bind<IGDPRExportService>().To<GDPRExportService>().InCommandScope(Mode);
            kernel.Bind<IFallbackUserResolver>().To<FallbackUserResolver>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationApplicationService>().To<DataProcessingRegistrationApplicationService>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationOptionsApplicationService>().To<DataProcessingRegistrationOptionsApplicationService>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationNamingService>().To<DataProcessingRegistrationNamingService>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationSystemAssignmentService>().To<DataProcessingRegistrationSystemAssignmentService>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationRoleAssignmentsService>().To<DataProcessingRegistrationRoleAssignmentsService>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationReadModelService>().To<DataProcessingRegistrationReadModelService>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationDataProcessorAssignmentService>().To<DataProcessingRegistrationDataProcessorAssignmentService>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationInsecureCountriesAssignmentService>().To<DataProcessingRegistrationInsecureCountriesAssignmentService>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationBasisForTransferAssignmentService>().To<DataProcessingRegistrationBasisForTransferAssignmentService>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationDataResponsibleAssignmentService>().To<DataProcessingRegistrationDataResponsibleAssigmentService>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationOversightOptionsAssignmentService>().To<DataProcessingRegistrationOversightOptionsAssignmentService>().InCommandScope(Mode);
            kernel.Bind<IContractDataProcessingRegistrationAssignmentService>().To<ContractDataProcessingRegistrationAssignmentService>().InCommandScope(Mode);
            kernel.Bind<IReadModelUpdate<DataProcessingRegistration, DataProcessingRegistrationReadModel>>().To<DataProcessingRegistrationReadModelUpdate>().InCommandScope(Mode);
            kernel.Bind<IItsystemUsageOverviewReadModelsService>().To<ItsystemUsageOverviewReadModelsService>().InCommandScope(Mode);
            kernel.Bind<IReadModelUpdate<ItSystemUsage, ItSystemUsageOverviewReadModel>>().To<ItSystemUsageOverviewReadModelUpdate>().InCommandScope(Mode);
            kernel.Bind<IKendoOrganizationalConfigurationService>().To<KendoOrganizationalConfigurationService>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationOversightDateAssignmentService>().To<DataProcessingRegistrationOversightDateAssignmentService>().InCommandScope(Mode);
            kernel.Bind<IHangfireApi>().To<HangfireApi>().InCommandScope(Mode);
            kernel.Bind<IOperationClock>().To<OperationClock>().InCommandScope(Mode);
            kernel.Bind<IUserNotificationService>().To<UserNotificationService>().InCommandScope(Mode);
            kernel.Bind<IUserNotificationApplicationService>().To<UserNotificationApplicationService>().InCommandScope(Mode);
            kernel.Bind<IBusinessTypeApplicationService>().To<BusinessTypeApplicationService>().InCommandScope(Mode);

            //MembershipProvider & Roleprovider injection - see ProviderInitializationHttpModule.cs
            kernel.Bind<MembershipProvider>().ToMethod(ctx => Membership.Provider);
            kernel.Bind<RoleProvider>().ToMethod(ctx => Roles.Provider);

            kernel.Bind<ILogger>().ToConstant(LogConfig.GlobalLogger).InTransientScope();
            kernel.Bind<IHttpModule>().To<ProviderInitializationHttpModule>();

            kernel.Bind<IOwinContext>().ToMethod(_ => HttpContext.Current?.GetOwinContext()).InCommandScope(Mode);
            kernel.Bind<Maybe<IOwinContext>>().ToMethod(_ => HttpContext.Current.FromNullable().Select(httpCtx => httpCtx.GetOwinContext())).InCommandScope(Mode);
            RegisterAuthenticationContext(kernel);
            RegisterAccessContext(kernel);
            RegisterKLE(kernel);
            RegisterOptions(kernel);
            RegisterBackgroundJobs(kernel);
            RegisterSSO(kernel);

            kernel.Bind<IRightsHoldersService>().To<RightsHoldersService>().InCommandScope(Mode);
        }

        private void RegisterSSO(IKernel kernel)
        {
            kernel.Bind<SsoFlowConfiguration>().ToMethod(_ => new SsoFlowConfiguration(Settings.Default.SsoServiceProviderId)).InSingletonScope();
            kernel.Bind<StsOrganisationIntegrationConfiguration>().ToMethod(_ =>
                new StsOrganisationIntegrationConfiguration(
                    Settings.Default.SsoCertificateThumbprint,
                    Settings.Default.StsOrganisationEndpointHost))
                .InSingletonScope();

            kernel.Bind<ISsoStateFactory>().To<SsoStateFactory>().InCommandScope(Mode);
            kernel.Bind<ISsoFlowApplicationService>().To<SsoFlowApplicationService>().InCommandScope(Mode);
            kernel.Bind<IStsBrugerInfoService>().To<StsBrugerInfoService>().InCommandScope(Mode);
            kernel.Bind<Maybe<ISaml20Identity>>().ToMethod(_ => Saml20Identity.IsInitialized() ? Saml20Identity.Current : Maybe<ISaml20Identity>.None).InCommandScope(Mode);
        }

        private void RegisterDomainEventsEngine(IKernel kernel)
        {
            kernel.Bind<IDomainEvents>().To<DomainEvents>().InCommandScope(Mode);
            RegisterDomainEvent<ExposingSystemChanged, RelationSpecificInterfaceEventsHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<ItInterface>, RelationSpecificInterfaceEventsHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<ItInterface>, UnbindBrokenReferenceReportsOnSourceDeletedHandler>(kernel);
            RegisterDomainEvent<AccessRightsChanged, ClearCacheOnAccessRightsChangedHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<ItSystemUsage>, UpdateRelationsOnSystemUsageDeletedHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<ExternalReference>, UnbindBrokenReferenceReportsOnSourceDeletedHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<ItSystemUsage>, CleanupDataProcessingRegistrationsOnSystemUsageDeletedEvent>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<DataProcessingRegistration>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityCreatedEvent<DataProcessingRegistration>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<DataProcessingRegistration>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<ExternalReference>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityCreatedEvent<ExternalReference>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ExternalReference>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<User>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<Organization>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EnabledStatusChanged<ItSystem>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<DataProcessingBasisForTransferOption>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<LocalDataProcessingBasisForTransferOption>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<DataProcessingDataResponsibleOption>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<LocalDataProcessingDataResponsibleOption>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<DataProcessingOversightOption>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<LocalDataProcessingOversightOption>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ItContract>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<ContractDeleted, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<ContractDeleted, ContractDeletedSystemRelationsHandler>(kernel);
            RegisterDomainEvent<ContractDeleted, ContractDeletedAdvicesHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<ItProject>, ProjectDeletedAdvicesHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<DataProcessingRegistration>, DataProcessingRegistrationDeletedAdvicesHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<ItSystemUsage>, SystemUsageDeletedAdvicesHandler>(kernel);

            RegisterDomainEvent<ContractDeleted, ContractDeletedUserNotificationsHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<ItProject>, ProjectDeletedUserNotificationsHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<DataProcessingRegistration>, DataProcessingRegistrationDeletedUserNotificationsHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<ItSystemUsage>, SystemUsageDeletedUserNotificationsHandler>(kernel);

            //Itsystem overview updates
            RegisterDomainEvent<EntityCreatedEvent<ItSystemUsage>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ItSystemUsage>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ItSystem>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<ItSystemUsage>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<User>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<OrganizationUnit>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<OrganizationUnit>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<Organization>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<Organization>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<BusinessType>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityCreatedEvent<LocalBusinessType>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<LocalBusinessType>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<TaskRef>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityCreatedEvent<ExternalReference>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ExternalReference>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<ExternalReference>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ItContract>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<ContractDeleted, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ItProject>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<ItProject>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<DataProcessingRegistration>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<DataProcessingRegistration>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ItInterface>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityDeletedEvent<ItInterface>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
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

            kernel.Bind<IOptionsService<DataProcessingRegistrationRight, DataProcessingRegistrationRole>>()
                .To<OptionsService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, LocalDataProcessingRegistrationRole>>().InCommandScope(Mode);

            kernel.Bind<IOptionsService<DataProcessingRegistration, DataProcessingCountryOption>>()
                .To<OptionsService<DataProcessingRegistration, DataProcessingCountryOption, LocalDataProcessingCountryOption>>().InCommandScope(Mode);

            kernel.Bind<IOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption>>()
                .To<OptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption, LocalDataProcessingBasisForTransferOption>>().InCommandScope(Mode);

            kernel.Bind<IOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption>>()
                .To<OptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption, LocalDataProcessingDataResponsibleOption>>().InCommandScope(Mode);

            kernel.Bind<IOptionsService<DataProcessingRegistration, DataProcessingOversightOption>>()
                .To<OptionsService<DataProcessingRegistration, DataProcessingOversightOption, LocalDataProcessingOversightOption>>().InCommandScope(Mode);

            kernel.Bind<IOptionsService<ItSystem, BusinessType>>()
               .To<OptionsService<ItSystem, BusinessType, LocalBusinessType>>().InCommandScope(Mode);

            kernel.Bind<IOptionsService<ItSystemRight, ItSystemRole>>()
                .To<OptionsService<ItSystemRight, ItSystemRole, LocalItSystemRole>>().InCommandScope(Mode);
        }

        private void RegisterKLE(IKernel kernel)
        {
            kernel.Bind<IKLEApplicationService>().To<KLEApplicationService>().InCommandScope(Mode);
            kernel.Bind<IKLEStandardRepository>().To<KLEStandardRepository>().InCommandScope(Mode);
            kernel.Bind<IKLEDataBridge>().To<KLEDataBridge>().InCommandScope(Mode);
            kernel.Bind<IKLEParentHelper>().To<KLEParentHelper>().InCommandScope(Mode);
            kernel.Bind<IKLEConverterHelper>().To<KLEConverterHelper>().InCommandScope(Mode);
            kernel.Bind<IKLEUpdateHistoryItemRepository>().To<KLEUpdateHistoryItemRepository>().InCommandScope(Mode);
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
            kernel.Bind<IDatabaseControl>().To<EntityFrameworkContextDatabaseControl>().InCommandScope(Mode);
            kernel.Bind<IItSystemUsageRepository>().To<ItSystemUsageRepository>().InCommandScope(Mode);
            kernel.Bind<IItProjectRepository>().To<ItProjectRepository>().InCommandScope(Mode);
            kernel.Bind<IInterfaceRepository>().To<InterfaceRepository>().InCommandScope(Mode);
            kernel.Bind<IReferenceRepository>().To<ReferenceRepository>().InCommandScope(Mode);
            kernel.Bind<IBrokenExternalReferencesReportRepository>().To<BrokenExternalReferencesReportRepository>().InCommandScope(Mode);
            kernel.Bind<IEntityTypeResolver>().To<PocoTypeFromProxyResolver>().InCommandScope(Mode);
            kernel.Bind<IOrganizationRepository>().To<OrganizationRepository>().InCommandScope(Mode);
            kernel.Bind<IOrganizationUnitRepository>().To<OrganizationUnitRepository>().InCommandScope(Mode);
            kernel.Bind<ISsoOrganizationIdentityRepository>().To<SsoOrganizationIdentityRepository>().InCommandScope(Mode);
            kernel.Bind<ISsoUserIdentityRepository>().To<SsoUserIdentityRepository>().InCommandScope(Mode);
            kernel.Bind<IAttachedOptionRepository>().To<AttachedOptionRepository>().InCommandScope(Mode);
            kernel.Bind<ISensitivePersonalDataTypeRepository>().To<SensitivePersonalDataTypeRepository>().InCommandScope(Mode);
            kernel.Bind<IAdviceRepository>().To<AdviceRepository>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationRepository>().To<DataProcessingRegistrationRepository>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationReadModelRepository>().To<DataProcessingRegistrationReadModelRepository>().InCommandScope(Mode);
            kernel.Bind<IPendingReadModelUpdateRepository>().To<PendingReadModelUpdateRepository>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationOptionRepository>().To<DataProcessingRegistrationOptionRepository>().InCommandScope(Mode);
            kernel.Bind<IItSystemUsageOverviewReadModelRepository>().To<ItSystemUsageOverviewReadModelRepository>().InCommandScope(Mode);
            kernel.Bind<IKendoOrganizationalConfigurationRepository>().To<KendoOrganizationalConfigurationRepository>().InCommandScope(Mode);

            kernel.Bind<IAdviceRootResolution>().To<AdviceRootResolution>().InCommandScope(Mode);
            kernel.Bind<IUserNotificationRepository>().To<UserNotificationRepository>().InCommandScope(Mode);
        }

        private void RegisterAuthenticationContext(IKernel kernel)
        {
            kernel.Bind<IApplicationAuthenticationState>().To<ApplicationAuthenticationState>().InCommandScope(Mode);
            kernel.Bind<IAuthenticationContextFactory>().To<OwinAuthenticationContextFactory>().InCommandScope(Mode);
            kernel.Bind<IAuthenticationContext>().ToMethod(ctx => ctx.Kernel.Get<IAuthenticationContextFactory>().Create()).InCommandScope(Mode);
        }

        private void RegisterAccessContext(IKernel kernel)
        {
            //User context
            kernel.Bind<UserContextFactory>().ToSelf();
            kernel.Bind<IUserContextFactory>().ToMethod(context =>
                    new CachingUserContextFactory(context.Kernel.GetRequiredService<UserContextFactory>(),
                        context.Kernel.GetRequiredService<IObjectCache>()))
                .InCommandScope(Mode);

            kernel.Bind<IOrganizationalUserContext>()
                .ToMethod(ctx =>
                {
                    var factory = ctx.Kernel.Get<IUserContextFactory>();
                    var authentication = ctx.Kernel.Get<IAuthenticationContext>();
                    var canCreateContext = authentication.Method != AuthenticationMethod.Anonymous;

                    if (canCreateContext)
                    {
                        return factory.Create(authentication.UserId.GetValueOrDefault());
                    }

                    return new UnauthenticatedUserContext();
                })
                .InCommandScope(Mode);

            //Injecting it as a maybe since service calls and background processes do not have an active user
            kernel.Bind<Maybe<ActiveUserIdContext>>()
                .ToMethod(ctx =>
                {
                    var authentication = ctx.Kernel.Get<IAuthenticationContext>();
                    if (authentication.UserId.HasValue == false || authentication.Method == AuthenticationMethod.Anonymous)
                    {
                        return Maybe<ActiveUserIdContext>.None;
                    }
                    return new ActiveUserIdContext(authentication.UserId.Value);
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
            kernel.Bind<IBackgroundJobLauncher>().To<BackgroundJobLauncher>().InCommandScope(Mode);
            kernel.Bind<IBackgroundJobScheduler>().To<BackgroundJobScheduler>().InCommandScope(Mode);
            kernel.Bind<CheckExternalLinksBackgroundJob>().ToSelf().InCommandScope(Mode);

            //DPR
            kernel.Bind<RebuildDataProcessingRegistrationReadModelsBatchJob>().ToSelf().InCommandScope(Mode);
            kernel.Bind<ScheduleDataProcessingRegistrationReadModelUpdates>().ToSelf().InCommandScope(Mode);

            //Itsystemusage
            kernel.Bind<RebuildItSystemUsageOverviewReadModelsBatchJob>().ToSelf().InCommandScope(Mode);
            kernel.Bind<ScheduleItSystemUsageOverviewReadModelUpdates>().ToSelf().InCommandScope(Mode);

            //Generic
            kernel.Bind<PurgeDuplicatePendingReadModelUpdates>().ToSelf().InCommandScope(Mode);

            //Rebuilder
            kernel.Bind<IRebuildReadModelsJobFactory>().To<RebuildReadModelsJobFactory>().InCommandScope(Mode);
        }
    }
}