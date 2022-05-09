﻿using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using AutoMapper;
using Core.Abstractions.Extensions;
using Core.Abstractions.Types;
using Core.ApplicationServices;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Contract.Write;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.GDPR.Write;
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
using Infrastructure.Services.Http;
using Infrastructure.Services.KLEDataBridge;
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
using Core.DomainServices.Repositories.TaskRefs;
using Core.ApplicationServices.Rights;
using Core.ApplicationServices.SystemUsage.ReadModels;
using Core.ApplicationServices.SystemUsage.Relations;
using Core.ApplicationServices.SystemUsage.Write;
using Core.DomainServices.Generic;
using Core.DomainServices.Organizations;
using Core.DomainServices.Role;
using Infrastructure.Ninject.ApplicationServices;
using Infrastructure.Ninject.DomainServices;
using Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping;
using Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Infrastructure.Model.Request;
using Core.ApplicationServices.Generic.Write;
using Core.ApplicationServices.Organizations.Handlers;
using Core.ApplicationServices.Tracking;
using Core.DomainServices.Tracking;
using Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping;
using Presentation.Web.Controllers.API.V2.External.ItInterfaces.Mapping;

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
            _builderActions.Add(kernel => kernel.Bind<ICurrentHttpRequest>().To<CurrentAspNetRequest>());
            _builderActions.Add(kernel => kernel.Bind<ICurrentRequestStream>().To<CurrentRequestStream>());

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
            kernel.Bind<IItsystemUsageRelationsService>().To<ItsystemUsageRelationsService>().InCommandScope(Mode);
            kernel.Bind<IItSystemUsageWriteService>().To<ItSystemUsageWriteService>().InCommandScope(Mode);
            kernel.Bind<IItInterfaceService>().To<ItInterfaceService>().InCommandScope(Mode);
            kernel.Bind<IItContractService>().To<ItContractService>().InCommandScope(Mode);
            kernel.Bind<IItContractWriteService>().To<ItContractWriteService>().InCommandScope(Mode);
            kernel.Bind<IUserRepositoryFactory>().To<UserRepositoryFactory>().InSingletonScope();
            kernel.Bind<IExcelService>().To<ExcelService>().InCommandScope(Mode);
            kernel.Bind<IExcelHandler>().To<ExcelHandler>().InCommandScope(Mode).Intercept().With(new LogInterceptor());
            kernel.Bind<IItSystemUsageMigrationService>().To<ItSystemUsageMigrationService>().InCommandScope(Mode);
            kernel.Bind<IReferenceService>().To<ReferenceService>().InCommandScope(Mode);
            kernel.Bind<IEndpointValidationService>().To<EndpointValidationService>().InCommandScope(Mode);
            kernel.Bind<IBrokenExternalReferencesReportService>().To<BrokenExternalReferencesReportService>().InCommandScope(Mode);
            kernel.Bind<IGDPRExportService>().To<GDPRExportService>().InCommandScope(Mode);
            kernel.Bind<IFallbackUserResolver>().To<FallbackUserResolver>().InCommandScope(Mode);
            kernel.Bind<IDefaultOrganizationResolver>().To<DefaultOrganizationResolver>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationWriteService>().To<DataProcessingRegistrationWriteService>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationApplicationService>().To<DataProcessingRegistrationApplicationService>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationOptionsApplicationService>().To<DataProcessingRegistrationOptionsApplicationService>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationNamingService>().To<DataProcessingRegistrationNamingService>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationSystemAssignmentService>().To<DataProcessingRegistrationSystemAssignmentService>().InCommandScope(Mode);
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
            kernel.Bind<IGlobalAdminNotificationService>().To<GlobalAdminNotificationService>().InCommandScope(Mode);
            kernel.Bind<IEntityIdentityResolver>().To<NinjectEntityIdentityResolver>().InCommandScope(Mode);
            kernel.Bind<IOptionResolver>().To<NinjectIOptionResolver>().InCommandScope(Mode);
            kernel.Bind<IAssignmentUpdateService>().To<AssignmentUpdateService>().InCommandScope(Mode);
            kernel.Bind<IEntityResolver>().To<NinjectEntityResolver>().InCommandScope(Mode);
            kernel.Bind<ITrackingService>().To<TrackingService>().InCommandScope(Mode);

            //Role assignment services
            RegisterRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage>(kernel);
            RegisterRoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, DataProcessingRegistration>(kernel);
            RegisterRoleAssignmentService<ItContractRight, ItContractRole, ItContract>(kernel);

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
            RegisterMappers(kernel);

            kernel.Bind<IRightsHolderSystemService>().To<RightsHolderSystemService>().InCommandScope(Mode);
            kernel.Bind<IItInterfaceRightsHolderService>().To<ItInterfaceRightsHolderService>().InCommandScope(Mode);
            kernel.Bind<IUserRightsService>().To<UserRightsService>().InCommandScope(Mode);
        }

        private void RegisterMappers(IKernel kernel)
        {
            //Systems
            kernel.Bind<IItSystemWriteModelMapper>().To<ItSystemWriteModelMapper>().InCommandScope(Mode);

            //System usage
            kernel.Bind<IItSystemUsageResponseMapper>().To<ItSystemUsageResponseMapper>().InCommandScope(Mode);
            kernel.Bind<IItSystemUsageWriteModelMapper>().To<ItSystemUsageWriteModelMapper>().InCommandScope(Mode);

            //Data processing
            kernel.Bind<IDataProcessingRegistrationWriteModelMapper>().To<DataProcessingRegistrationWriteModelMapper>().InCommandScope(Mode);
            kernel.Bind<IDataProcessingRegistrationResponseMapper>().To<DataProcessingRegistrationResponseMapper>().InCommandScope(Mode);

            //Contracts
            kernel.Bind<IItContractWriteModelMapper>().To<ItContractWriteModelMapper>().InCommandScope(Mode);
            kernel.Bind<IItContractResponseMapper>().To<ItContractResponseMapper>().InCommandScope(Mode);

            //Interfaces
            kernel.Bind<IItInterfaceWriteModelMapper>().To<ItInterfaceWriteModelMapper>().InCommandScope(Mode);
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
            kernel.Bind<IDomainEvents>().To<NinjectDomainEventsAdapter>().InCommandScope(Mode);
            RegisterDomainEvent<ExposingSystemChanged, RelationSpecificInterfaceEventsHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItInterface>, RelationSpecificInterfaceEventsHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItInterface>, UnbindBrokenReferenceReportsOnSourceDeletedHandler>(kernel);
            RegisterDomainEvent<AccessRightsChanged, ClearCacheOnAccessRightsChangedHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItSystemUsage>, UpdateRelationsOnSystemUsageDeletedHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ExternalReference>, UnbindBrokenReferenceReportsOnSourceDeletedHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItSystemUsage>, CleanupDataProcessingRegistrationsOnSystemUsageDeletedEvent>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<DataProcessingRegistration>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityCreatedEvent<DataProcessingRegistration>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<DataProcessingRegistration>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ExternalReference>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
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
            RegisterDomainEvent<EntityBeingDeletedEvent<ItContract>, BuildDataProcessingRegistrationReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItContract>, ContractDeletedSystemRelationsHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItContract>, ContractDeletedAdvicesHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItProject>, ProjectDeletedAdvicesHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<DataProcessingRegistration>, DataProcessingRegistrationDeletedAdvicesHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItSystemUsage>, SystemUsageDeletedAdvicesHandler>(kernel);

            RegisterDomainEvent<EntityBeingDeletedEvent<ItContract>, ContractDeletedUserNotificationsHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItProject>, ProjectDeletedUserNotificationsHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<DataProcessingRegistration>, DataProcessingRegistrationDeletedUserNotificationsHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItSystemUsage>, SystemUsageDeletedUserNotificationsHandler>(kernel);

            //Itsystem overview updates
            RegisterDomainEvent<EntityCreatedEvent<ItSystemUsage>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ItSystemUsage>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ItSystem>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItSystemUsage>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<User>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<OrganizationUnit>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<OrganizationUnit>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<Organization>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<Organization>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<BusinessType>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityCreatedEvent<LocalBusinessType>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<LocalBusinessType>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<TaskRef>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityCreatedEvent<ExternalReference>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ExternalReference>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ExternalReference>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ItContract>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItContract>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ItProject>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItProject>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<DataProcessingRegistration>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<DataProcessingRegistration>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ItInterface>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItInterface>, BuildItSystemUsageOverviewReadModelOnChangesHandler>(kernel);

            //Dirty marking
            RegisterDomainEvent<EntityUpdatedEvent<ItInterface>, MarkEntityAsDirtyOnChangeEventHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ItSystemUsage>, MarkEntityAsDirtyOnChangeEventHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ItSystem>, MarkEntityAsDirtyOnChangeEventHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<ItContract>, MarkEntityAsDirtyOnChangeEventHandler>(kernel);
            RegisterDomainEvent<EntityUpdatedEvent<DataProcessingRegistration>, MarkEntityAsDirtyOnChangeEventHandler>(kernel);

            //Deletion tracking
            RegisterDomainEvent<EntityBeingDeletedEvent<ItProject>, TrackDeletedEntitiesEventHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItInterface>, TrackDeletedEntitiesEventHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItSystemUsage>, TrackDeletedEntitiesEventHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItSystem>, TrackDeletedEntitiesEventHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<ItContract>, TrackDeletedEntitiesEventHandler>(kernel);
            RegisterDomainEvent<EntityBeingDeletedEvent<DataProcessingRegistration>, TrackDeletedEntitiesEventHandler>(kernel);

            //Organization
            RegisterDomainEvent<EntityBeingDeletedEvent<Organization>, HandleOrganizationBeingDeleted>(kernel);
            //TODO: Read models where it is involved must be scheduled for rebuild.. do it in a different handler (one of the read model handlers)
        }

        private void RegisterDomainEvent<TDomainEvent, THandler>(IKernel kernel)
            where TDomainEvent : IDomainEvent
            where THandler : IDomainEventHandler<TDomainEvent>
        {
            kernel.Bind<IDomainEventHandler<TDomainEvent>>().To<THandler>().InCommandScope(Mode);
        }

        private void RegisterOptions(IKernel kernel)
        {
            //Data processing registrations
            RegisterOptionsService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, LocalDataProcessingRegistrationRole>(kernel);

            RegisterOptionsService<DataProcessingRegistration, DataProcessingCountryOption, LocalDataProcessingCountryOption>(kernel);

            RegisterOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption, LocalDataProcessingBasisForTransferOption>(kernel);

            RegisterOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption, LocalDataProcessingDataResponsibleOption>(kernel);

            RegisterOptionsService<DataProcessingRegistration, DataProcessingOversightOption, LocalDataProcessingOversightOption>(kernel);

            //IT-System
            RegisterOptionsService<ItSystem, BusinessType, LocalBusinessType>(kernel);

            RegisterOptionsService<ItSystem, SensitivePersonalDataType, LocalSensitivePersonalDataType>(kernel);

            //IT-System usages
            RegisterOptionsService<ItSystemRight, ItSystemRole, LocalItSystemRole>(kernel);

            RegisterOptionsService<SystemRelation, RelationFrequencyType, LocalRelationFrequencyType>(kernel);

            RegisterOptionsService<ItSystemUsage, ItSystemCategories, LocalItSystemCategories>(kernel);

            RegisterOptionsService<ItSystemUsage, ArchiveType, LocalArchiveType>(kernel);

            RegisterOptionsService<ItSystemUsage, ArchiveLocation, LocalArchiveLocation>(kernel);

            RegisterOptionsService<ItSystemUsage, ArchiveTestLocation, LocalArchiveTestLocation>(kernel);

            RegisterOptionsService<ItSystemUsage, RegisterType, LocalRegisterType>(kernel);

            //IT-Contract
            RegisterOptionsService<ItContractRight, ItContractRole, LocalItContractRole>(kernel);

            RegisterOptionsService<ItContract, ItContractType, LocalItContractType>(kernel);

            RegisterOptionsService<ItContract, ItContractTemplateType, LocalItContractTemplateType>(kernel);

            RegisterOptionsService<ItContract, PurchaseFormType, LocalPurchaseFormType>(kernel);

            RegisterOptionsService<ItContract, PaymentModelType, LocalPaymentModelType>(kernel);

            RegisterOptionsService<ItContract, AgreementElementType, LocalAgreementElementType>(kernel);

            RegisterOptionsService<ItContract, PaymentFreqencyType, LocalPaymentFreqencyType>(kernel);

            RegisterOptionsService<ItContract, PriceRegulationType, LocalPriceRegulationType>(kernel);

            RegisterOptionsService<ItContract, ProcurementStrategyType, LocalProcurementStrategyType>(kernel);

            RegisterOptionsService<ItContract, OptionExtendType, LocalOptionExtendType>(kernel);

            RegisterOptionsService<ItContract, TerminationDeadlineType, LocalTerminationDeadlineType>(kernel);

            RegisterOptionsService<HandoverTrial, HandoverTrialType, LocalHandoverTrialType>(kernel);

            //Attached options services
            kernel.Bind<IAttachedOptionsAssignmentService<RegisterType, ItSystemUsage>>().ToMethod(ctx =>
                new AttachedOptionsAssignmentService<RegisterType, ItSystemUsage>(OptionType.REGISTERTYPEDATA,
                    ctx.Kernel.GetRequiredService<IItSystemUsageAttachedOptionRepository>(),
                    ctx.Kernel.GetRequiredService<IOptionsService<ItSystemUsage, RegisterType>>()));

            kernel.Bind<IAttachedOptionsAssignmentService<SensitivePersonalDataType, ItSystem>>().ToMethod(ctx =>
                new AttachedOptionsAssignmentService<SensitivePersonalDataType, ItSystem>(OptionType.SENSITIVEPERSONALDATA,
                    ctx.Kernel.GetRequiredService<IItSystemUsageAttachedOptionRepository>(),
                    ctx.Kernel.GetRequiredService<IOptionsService<ItSystem, SensitivePersonalDataType>>()));
        }

        private void RegisterOptionsService<TParent, TOption, TLocalOption>(IKernel kernel)
            where TOption : OptionEntity<TParent>
            where TLocalOption : LocalOptionEntity<TOption>
        {
            //Domain service
            kernel.Bind<IOptionsService<TParent, TOption>>()
                .To<OptionsService<TParent, TOption, TLocalOption>>().InCommandScope(Mode);

            //Application service
            kernel.Bind<IOptionsApplicationService<TParent, TOption>>()
                .To<OptionsApplicationService<TParent, TOption>>().InCommandScope(Mode);
        }

        private void RegisterRoleAssignmentService<TRight, TRole, TModel>(IKernel kernel)
            where TRight : Entity, IRight<TModel, TRight, TRole>
            where TRole : OptionEntity<TRight>, IRoleEntity, IOptionReference<TRight>
            where TModel : HasRightsEntity<TModel, TRight, TRole>, IOwnedByOrganization
        {
            kernel.Bind<IRoleAssignmentService<TRight, TRole, TModel>>()
                .To<RoleAssignmentService<TRight, TRole, TModel>>().InCommandScope(Mode);
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
            kernel.Bind<IItSystemUsageAttachedOptionRepository>().To<ItSystemUsageAttachedOptionRepository>().InCommandScope(Mode);
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
            kernel.Bind<ITaskRefRepository>().To<TaskRefRepository>().InCommandScope(Mode);
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