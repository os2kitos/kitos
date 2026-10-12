using System;
using System.IO;
using System.Linq;
using AutoMapper;
using Npgsql;
using Core.Abstractions.Caching;
using Core.Abstractions.Helpers;
using Core.Abstractions.Types;
using Infrastructure.DataAccess.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using Core.ApplicationServices;
using Core.ApplicationServices.Authentication;
using Core.ApplicationServices.Authorization;
using Core.ApplicationServices.Contract;
using Core.ApplicationServices.Contract.ReadModels;
using Core.ApplicationServices.Contract.Write;
using Core.ApplicationServices.GDPR;
using Core.ApplicationServices.GDPR.Write;
using Core.ApplicationServices.Generic;
using Core.ApplicationServices.Generic.Write;
using Core.ApplicationServices.GlobalOptions;
using Core.ApplicationServices.HelpTexts;
using Core.ApplicationServices.Interface;
using Core.ApplicationServices.Interface.Write;
using Core.ApplicationServices.KitosEvents;
using Core.ApplicationServices.KLE;
using Core.ApplicationServices.LocalOptions;
using Core.ApplicationServices.Mapping.Authorization;
using Core.ApplicationServices.Messages;
using Core.ApplicationServices.Model.KitosEvents;
using Core.ApplicationServices.Notification;
using Core.ApplicationServices.OptionTypes;
using Core.ApplicationServices.Organizations;
using Core.ApplicationServices.Organizations.Handlers;
using Core.ApplicationServices.Organizations.Write;
using Core.ApplicationServices.Qa;
using Core.ApplicationServices.References;
using Core.ApplicationServices.RightsHolders;
using Core.ApplicationServices.Rights;
using Core.ApplicationServices.ScheduledJobs;
using Core.ApplicationServices.SSO;
using Core.ApplicationServices.SSO.Factories;
using Core.ApplicationServices.System;
using Core.ApplicationServices.System.Write;
using Core.ApplicationServices.SystemUsage;
using Core.ApplicationServices.SystemUsage.GDPR;
using Core.ApplicationServices.SystemUsage.Migration;
using Core.ApplicationServices.SystemUsage.ReadModels;
using Core.ApplicationServices.SystemUsage.Relations;
using Core.ApplicationServices.SystemUsage.Write;
using Core.ApplicationServices.Tracking;
using Core.ApplicationServices.UIConfiguration;
using Core.ApplicationServices.Users.Handlers;
using Core.ApplicationServices.Users.Write;
using Core.BackgroundJobs.Factories;
using Core.BackgroundJobs.Model.ExternalLinks;
using Core.BackgroundJobs.Model.Maintenance;
using Core.BackgroundJobs.Model.PublicMessages;
using Core.BackgroundJobs.Model.ReadModels;
using Core.BackgroundJobs.Services;
using Core.DomainModel;
using Core.DomainModel.Events;
using Core.DomainModel.Commands;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItContract.Read;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.ItSystemUsage.Read;
using Core.DomainModel.LocalOptions;
using Core.DomainModel.Organization;
using Core.DomainServices;
using Core.DomainServices.Advice;
using Core.DomainServices.Context;
using Core.DomainServices.Contract;
using Core.DomainServices.GDPR;
using Core.DomainServices.Generic;
using Core.DomainServices.Model;
using Core.DomainServices.Model.EventHandlers;
using Core.DomainServices.Notifications;
using Core.DomainServices.Options;
using Core.DomainServices.Organizations;
using Core.DomainServices.Repositories.Advice;
using Core.DomainServices.Repositories.Contract;
using Core.DomainServices.Repositories.GDPR;
using Core.DomainServices.Repositories.Interface;
using Core.DomainServices.Repositories.Kendo;
using Core.DomainServices.Repositories.KLE;
using Core.DomainServices.Repositories.Notification;
using Core.DomainServices.Repositories.Organization;
using Core.DomainServices.Repositories.Qa;
using Core.DomainServices.Repositories.Reference;
using Core.DomainServices.Repositories.SSO;
using Core.DomainServices.Repositories.System;
using Core.DomainServices.Repositories.SystemUsage;
using Core.DomainServices.Repositories.TaskRefs;
using Core.DomainServices.Repositories.UICustomization;
using Core.DomainServices.Repositories.BackgroundJobs;
using Core.DomainServices.Role;
using Core.DomainServices.SSO;
using Core.DomainServices.SystemUsage;
using Core.DomainServices.Time;
using Core.DomainServices.Tracking;
using Core.DomainServices.Suppliers;
using Infrastructure.DataAccess;
using Infrastructure.DataAccess.Repositories.SystemUsage;
using Infrastructure.DataAccess.Services;
using Infrastructure.Ninject.ApplicationServices;
using Infrastructure.Services.KLEDataBridge;
using Infrastructure.Ninject.DomainServices;
using Infrastructure.OpenXML;
using Infrastructure.Services.BackgroundJobs;
using Infrastructure.Services.Caching;
using Infrastructure.Services.Configuration;
using Infrastructure.Services.Cryptography;
using Infrastructure.Services.DataAccess;
using Infrastructure.Services.Http;
using Infrastructure.Services.Types;
using Infrastructure.STS.Company.DomainServices;
using Infrastructure.STS.Organization.DomainServices;
using Infrastructure.STS.OrganizationSystem.DomainServices;
using Kombit.InfrastructureSamples.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Web.Controllers.API.V2.Common.Mapping;
using Presentation.Web.Controllers.API.V2.External.DataProcessingRegistrations.Mapping;
using Presentation.Web.Controllers.API.V2.External.Generic;
using Presentation.Web.Controllers.API.V2.External.ItContracts.Mapping;
using Presentation.Web.Controllers.API.V2.External.ItInterfaces.Mapping;
using Presentation.Web.Controllers.API.V2.External.ItSystems.Mapping;
using Presentation.Web.Controllers.API.V2.External.ItSystemUsages.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.ItSystemUsages.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Messages.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Notifications.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.OrganizationUnits.Mapping;
using Presentation.Web.Controllers.API.V2.Internal.Users.Mapping;
using Presentation.Web.Infrastructure.Factories.Authentication;
using Presentation.Web.Infrastructure.Mail;
using Presentation.Web.Infrastructure.Model.Request;
using Serilog;
using Core.ApplicationServices.Model.EventHandler;
using dk.nita.saml20.identity;
using Presentation.Web.Infrastructure.Middleware;
using ApplicationAuthenticationState = Presentation.Web.Infrastructure.Authentication.ApplicationAuthenticationState;
using Core.ApplicationServices.Users;
using Core.ApplicationServices.Model.SystemUsage;

namespace Presentation.Web.Infrastructure.DI
{
    public static class KitosServiceRegistration
    {
        public static void Register(IServiceCollection services, IConfiguration configuration, SecurityKey signingKey)
        {
            // Middleware (IMiddleware implementations must be registered in DI)
            services.AddScoped<NormalizeODataQueryStringMiddleware>();
            services.AddScoped<CorrelationIdMiddleware>();
            services.AddScoped<ApiRequestsLoggingMiddleware>();
            services.AddScoped<DenyUsersWithoutApiAccessMiddleware>();
            services.AddScoped<DenyModificationsThroughApiMiddleware>();
            services.AddScoped<DenyTooLargeQueriesMiddleware>();

            // Logger
            services.AddSingleton(Log.Logger);

            // Http request services
            services.AddScoped<ICurrentHttpRequest, CurrentAspNetCoreRequest>();
            services.AddScoped<ICurrentRequestStream, CurrentRequestStream>();

            // Authentication context
            services.AddScoped<IAuthenticationContextFactory, OwinAuthenticationContextFactory>();
            services.AddScoped<IAuthenticationContext>(sp =>
                sp.GetRequiredService<IAuthenticationContextFactory>().Create());

            // AutoMapper
            services.AddSingleton<IMapper>(_ => MappingConfig.CreateMapper());

            // Object cache
            services.AddSingleton<IObjectCache, AspNetObjectCache>();

            // Config-driven
            var appSettings = configuration.GetSection("AppSettings");
            var baseUrl = appSettings["BaseUrl"] ?? "https://localhost:44300/";
            var mailSuffix = appSettings["MailSuffix"] ?? "@test.dk";
            var defaultUserPassword = appSettings["DefaultUserPassword"] ?? "";
            var useDefaultUserPassword = bool.Parse(appSettings["UseDefaultUserPassword"] ?? "false");
            var resetPasswordTtl = TimeSpan.Parse(appSettings["ResetPasswordTTL"] ?? "24:00:00");

            services.AddSingleton(_ => new KitosUrl(new Uri(baseUrl)));
            services.AddScoped<IMailClient>(_ =>
            {
                var smtpSection = configuration.GetSection("Smtp");
                var fromAddress = smtpSection["From"] ?? "noreply@kitos.dk";
                var deliveryMethod = smtpSection["DeliveryMethod"] ?? "SpecifiedPickupDirectory";

                SingleThreadedMailClient inner;
                if (deliveryMethod.Equals("SpecifiedPickupDirectory", StringComparison.OrdinalIgnoreCase))
                {
                    var configuredPickupDir = smtpSection["PickupDirectoryLocation"];
                    var pickupDir = string.IsNullOrWhiteSpace(configuredPickupDir)
                        ? Path.Combine(Path.GetTempPath(), "kitos-maildrop")
                        : configuredPickupDir;

                    try
                    {
                        Directory.CreateDirectory(pickupDir);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Unable to create SMTP pickup directory '{pickupDir}'. Check Smtp:PickupDirectoryLocation.", ex);
                    }

                    inner = new SingleThreadedMailClient(pickupDir);
                }
                else
                {
                    var host = smtpSection["Host"];
                    if (string.IsNullOrWhiteSpace(host))
                        throw new InvalidOperationException("Smtp:Host is required when DeliveryMethod is Network");
                    var port = int.Parse(smtpSection["Port"] ?? "25");
                    var ssl = bool.Parse(smtpSection["EnableSsl"] ?? "false");
                    var userName = smtpSection["UserName"];
                    var password = smtpSection["Password"];
                    inner = new SingleThreadedMailClient(host, port, ssl, userName, password);
                }

                return new DefaultFromAddressMailClient(inner, fromAddress);
            });
            services.AddScoped<ICryptoService, CryptoService>();
            services.AddScoped<IUserService>(sp => new UserService(
                resetPasswordTtl,
                baseUrl,
                mailSuffix,
                defaultUserPassword,
                useDefaultUserPassword,
                sp.GetRequiredService<IGenericRepository<User>>(),
                sp.GetRequiredService<IGenericRepository<Organization>>(),
                sp.GetRequiredService<IGenericRepository<PasswordResetRequest>>(),
                sp.GetRequiredService<IMailClient>(),
                sp.GetRequiredService<ICryptoService>(),
                sp.GetRequiredService<IAuthorizationContext>(),
                sp.GetRequiredService<IDomainEvents>(),
                sp.GetRequiredService<IUserRepository>(),
                sp.GetRequiredService<IOrganizationService>(),
                sp.GetRequiredService<ITransactionManager>(),
                sp.GetRequiredService<IOrganizationalUserContext>(),
                sp.GetRequiredService<ICommandBus>()
            ));

            services.AddScoped<IUserWriteService, UserWriteService>();
            services.AddScoped<IUserLoggedInService, UserLoggedInService>();
            services.AddScoped<IOrgUnitService, OrgUnitService>();
            services.AddScoped<IOrganizationRoleService, OrganizationRoleService>();
            services.AddScoped<IOrganizationRightsService, OrganizationRightsService>();
            services.AddScoped<IAdviceService, AdviceService>();
            services.AddScoped<AdviceService>();
            services.AddScoped<IRegistrationNotificationService, RegistrationNotificationService>();
            services.AddScoped<IRegistrationNotificationUserRelationsService, RegistrationNotificationUserRelationsService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<IOrganizationWriteService, OrganizationWriteService>();
            services.AddScoped<IOrganizationSupplierService, OrganizationSupplierService>();
            services.AddScoped<IItSystemService, ItSystemService>();
            services.AddScoped<IItSystemUsageService, ItSystemUsageService>();
            services.AddScoped<IItSystemUsageArchiveService, ItSystemUsageArchiveService>();
            services.AddScoped<IItSystemUsageMigrationServiceAdapter, ItSystemUsageMigrationServiceAdapter>();
            services.AddScoped<IItsystemUsageRelationsService, ItsystemUsageRelationsService>();
            services.AddScoped<IItSystemUsageWriteService, ItSystemUsageWriteService>();
            services.AddScoped<IItInterfaceService, ItInterfaceService>();
            services.AddScoped<IItContractService, ItContractService>();
            services.AddScoped<IItContractWriteService, ItContractWriteService>();
            services.AddScoped<IItContractOverviewReadModelsService, ItContractOverviewReadModelsService>();
            services.AddScoped<IItContractSupplierOverviewReadModelsService, ItContractSupplierOverviewReadModelsService>();
            services.AddScoped<IReadModelUpdate<ItContract, ItContractOverviewReadModel>, ItContractOverviewReadModelUpdate>();
            services.AddSingleton<IUserRepositoryFactory, UserRepositoryFactory>();
            services.AddScoped<IExcelService, ExcelService>();
            services.AddScoped<IExcelHandler, ExcelHandler>();
            services.AddScoped<IItSystemUsageMigrationService, ItSystemUsageMigrationService>();
            services.AddScoped<IReferenceService, ReferenceService>();
            services.AddScoped<IEndpointValidationService, EndpointValidationService>();
            services.AddScoped<IEndpointValidationConfiguration, EndpointValidationConfiguration>();
            services.AddScoped<IBrokenExternalReferencesReportService, BrokenExternalReferencesReportService>();
            services.AddScoped<IGDPRExportService, GDPRExportService>();
            services.AddScoped<IFallbackUserResolver, FallbackUserResolver>();
            services.AddScoped<IDefaultOrganizationResolver, DefaultOrganizationResolver>();
            services.AddScoped<IDataProcessingRegistrationWriteService, DataProcessingRegistrationWriteService>();
            services.AddScoped<IDataProcessingRegistrationApplicationService, DataProcessingRegistrationApplicationService>();
            services.AddScoped<IDataProcessingRegistrationOptionsApplicationService, DataProcessingRegistrationOptionsApplicationService>();
            services.AddScoped<IDataProcessingRegistrationNamingService, DataProcessingRegistrationNamingService>();
            services.AddScoped<IDataProcessingRegistrationSystemAssignmentService, DataProcessingRegistrationSystemAssignmentService>();
            services.AddScoped<IDataProcessingRegistrationReadModelService, DataProcessingRegistrationReadModelService>();
            services.AddScoped<IDataProcessingRegistrationDataProcessorAssignmentService, DataProcessingRegistrationDataProcessorAssignmentService>();
            services.AddScoped<IDataProcessingRegistrationInsecureCountriesAssignmentService, DataProcessingRegistrationInsecureCountriesAssignmentService>();
            services.AddScoped<IDataProcessingRegistrationBasisForTransferAssignmentService, DataProcessingRegistrationBasisForTransferAssignmentService>();
            services.AddScoped<IDataProcessingRegistrationDataResponsibleAssignmentService, DataProcessingRegistrationDataResponsibleAssigmentService>();
            services.AddScoped<IDataProcessingRegistrationOversightOptionsAssignmentService, DataProcessingRegistrationOversightOptionsAssignmentService>();
            services.AddScoped<IContractDataProcessingRegistrationAssignmentService, ContractDataProcessingRegistrationAssignmentService>();
            services.AddScoped<IReadModelUpdate<DataProcessingRegistration, DataProcessingRegistrationReadModel>, DataProcessingRegistrationReadModelUpdate>();
            services.AddScoped<IItsystemUsageOverviewReadModelsService, ItsystemUsageOverviewReadModelsService>();
            services.AddScoped<IReadModelUpdate<ItSystemUsage, ItSystemUsageOverviewReadModel>, ItSystemUsageOverviewReadModelUpdate>();
            services.AddScoped<IKendoOrganizationalConfigurationService, KendoOrganizationalConfigurationService>();
            services.AddScoped<IHangfireApi, HangfireApi>();
            services.AddScoped<IOperationClock, OperationClock>();
            services.AddScoped<IUserNotificationService, UserNotificationService>();
            services.AddScoped<IUserNotificationApplicationService, UserNotificationApplicationService>();
            services.AddScoped<IGlobalAdminNotificationService, GlobalAdminNotificationService>();
            services.AddScoped<IEntityIdentityResolver, NinjectEntityIdentityResolver>();
            services.AddScoped<IOptionResolver, NinjectIOptionResolver>();
            services.AddScoped<IAssignmentUpdateService, AssignmentUpdateService>();
            services.AddScoped<IEntityResolver, NinjectEntityResolver>();
            services.AddScoped<IEntityTypeResolver, PocoTypeFromProxyResolver>();
            services.AddScoped<ITrackingService, TrackingService>();
            services.AddScoped<IAdviceRootResolution, AdviceRootResolution>();
            services.AddScoped<ISupplierAssociatedFieldKeyMapper, SupplierAssociatedFieldKeyMapper>();
            services.AddScoped<ISupplierFieldDomainService, SupplierFieldDomainService>();
            services.AddScoped<ISupplierAssociatedFieldsService, SupplierAssociatedFieldsService>();
            services.AddScoped<IUIModuleCustomizationService, UIModuleCustomizationService>();
            services.AddScoped<IOrganizationUnitService, OrganizationUnitService>();
            services.AddScoped<IOrganizationUnitWriteService, OrganizationUnitWriteService>();
            services.AddScoped<IEntityIdMapper, EntityIdMapper>();
            services.AddScoped<IEntityTreeUuidCollector, EntityTreeUuidCollector>();
            services.AddScoped<IRightsHolderSystemService, RightsHolderSystemService>();
            services.AddScoped<IItSystemWriteService, ItSystemWriteService>();
            services.AddScoped<IItInterfaceRightsHolderService, ItInterfaceRightsHolderService>();
            services.AddScoped<IItInterfaceWriteService, ItInterfaceWriteService>();
            services.AddScoped<IUserRightsService, UserRightsService>();
            services.AddScoped<IPublicMessagesService, PublicMessagesService>();
            services.AddScoped<IHelpTextService, HelpTextService>();
            services.AddScoped<IHelpTextApplicationService, HelpTextApplicationService>();
            services.AddScoped<IKitosEventPublisherService, KitosEventPublisherService>();
            services.AddScoped<IKitosEventMapper, KitosEventMapper>();

            var pubSubBaseUrl = appSettings["PubSubBaseUrl"] ?? "";
            services.AddScoped<IKitosHttpClient, KitosHttpClient>();
            services.AddScoped<IHttpEventPublisher>(sp => new HttpEventPublisher(
                sp.GetRequiredService<IKitosHttpClient>(),
                sp.GetRequiredService<IKitosInternalTokenIssuer>(),
                pubSubBaseUrl));

            services.AddScoped<ITokenValidator>(sp => new TokenValidator(baseUrl, signingKey));
            services.AddScoped<IKitosInternalTokenIssuer, KitosInternalTokenIssuer>();

            // STS Organization
            var ssoCertificateThumbprint = appSettings["SsoCertificateThumbprint"] ?? "";
            var stsIssuer = appSettings["StsIssuer"] ?? "";
            var stsCertificateEndpoint = appSettings["StsCertificateEndpoint"] ?? "";
            var stsCertificateAlias = appSettings["StsCertificateAlias"] ?? "";
            var stsCertificateThumbprint = appSettings["StsCertificateThumbprint"] ?? "";
            var serviceCertificateAliasOrg = appSettings["ServiceCertificateAliasOrg"] ?? "";
            var orgService6EntityId = appSettings["OrgService6EntityId"] ?? "";
            var ssoServiceProviderId = appSettings["SsoServiceProviderId"] ?? "";
            var stsOrganisationCertificateThumbprint = appSettings["StsOrganisationCertificateThumbprint"] ?? "";
            var stsVirksomhedPort = appSettings["StsVirksomhedPort"] ?? "";
            var stsOrganisationPort = appSettings["StsOrganisationPort"] ?? "";
            var stsOrganisationSystemPort = appSettings["StsOrganisationSystemPort"] ?? "";
            var stsAdressePort = appSettings["StsAdressePort"] ?? "";
            var stsBrugerPort = appSettings["StsBrugerPort"] ?? "";
            var stsPersonPort = appSettings["StsPersonPort"] ?? "";

            // Certificate file paths for container environments (empty = use Windows cert store)
            var ssoCertFilePath = appSettings["SsoCertFilePath"] ?? "";
            var ssoCertPassword = appSettings["SsoCertPassword"] ?? "";
            var stsCertFilePath = appSettings["StsCertFilePath"] ?? "";
            var stsCertPassword = appSettings["StsCertPassword"] ?? "";
            var stsOrganisationCertFilePath = appSettings["StsOrganisationCertFilePath"] ?? "";
            var stsOrganisationCertPassword = appSettings["StsOrganisationCertPassword"] ?? "";
            var stsCertificateValidationMode = appSettings["StsCertificateValidationMode"] ?? "";
            var stsCertificateRevocationMode = appSettings["StsCertificateRevocationMode"] ?? "";

            var tokenFetcherConfig = new TokenFetcherConfig(
                ssoCertificateThumbprint, stsIssuer, stsCertificateEndpoint,
                stsCertificateAlias, stsCertificateThumbprint, stsOrganisationCertificateThumbprint,
                ssoCertFilePath, ssoCertPassword,
                stsCertFilePath, stsCertPassword,
                stsOrganisationCertFilePath, stsOrganisationCertPassword,
                stsCertificateValidationMode, stsCertificateRevocationMode);
            services.AddSingleton(_ => new TokenFetcher(tokenFetcherConfig));
            services.AddScoped<IStsOrganizationService, StsOrganizationService>();
            services.AddScoped<IStsOrganizationCompanyLookupService, StsOrganizationCompanyLookupService>();
            services.AddScoped<IStsOrganizationSystemService, StsOrganizationSystemService>();
            services.AddScoped<IStsOrganizationSynchronizationService, StsOrganizationSynchronizationService>();

            // SSO
            services.AddSingleton(_ => new SsoFlowConfiguration(ssoServiceProviderId));
            services.AddSingleton(_ => new StsOrganisationIntegrationConfiguration(
                ssoCertificateThumbprint, stsIssuer,
                stsCertificateEndpoint, serviceCertificateAliasOrg, stsCertificateAlias,
                stsCertificateThumbprint, orgService6EntityId,
                stsVirksomhedPort, stsOrganisationPort, stsOrganisationSystemPort,
                stsAdressePort, stsBrugerPort, stsPersonPort));
            services.AddScoped<ISsoStateFactory, SsoStateFactory>();
            services.AddScoped<ISsoFlowApplicationService, SsoFlowApplicationService>();

            var localSsoTestEmail = appSettings["LocalSsoTestUserEmail"];
            if (!string.IsNullOrWhiteSpace(localSsoTestEmail))
            {
                // Local-dev bypass: skip KOMBIT STS BrugerService call (see LocalTestStsBrugerInfoService).
                // Remove or clear "LocalSsoTestUserEmail" in Web.config before deploying to production.
                services.AddScoped<IStsBrugerInfoService>(sp =>
                    new Presentation.Web.Infrastructure.SSO.LocalTestStsBrugerInfoService(
                        localSsoTestEmail, sp.GetRequiredService<ILogger>()));
            }
            else
            {
                services.AddScoped<IStsBrugerInfoService, StsBrugerInfoService>();
            }

            services.AddScoped<IApplicationAuthenticationState, ApplicationAuthenticationState>();
            services.AddScoped<Maybe<ActiveUserIdContext>>(sp =>
            {
                var authentication = sp.GetRequiredService<IAuthenticationContext>();
                if (!authentication.UserId.HasValue || authentication.Method == AuthenticationMethod.Anonymous)
                    return Maybe<ActiveUserIdContext>.None;
                return new ActiveUserIdContext(authentication.UserId.Value);
            });
            services.AddScoped<Maybe<ISaml20Identity>>(sp =>
            {
                // Explicitly pin the current request's HttpContext so the SAML session lookup
                // succeeds for ordinary controller requests (e.g. GET /SSO) where
                // Saml20AbstractEndpointHandler.ProcessRequest has not been called and therefore
                // the AsyncLocal in SamlHttpContextAccessor has not been set.
                var httpCtx = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;
                if (httpCtx != null)
                    dk.nita.saml20.Utils.SamlHttpContextAccessor.SetExplicitContext(httpCtx);

                if (!Saml20Identity.IsInitialized()) return Maybe<ISaml20Identity>.None;
                var current = Saml20Identity.Current;
                return current != null ? (Maybe<ISaml20Identity>)current : Maybe<ISaml20Identity>.None;
            });

            RegisterDataAccess(services, configuration);
            RegisterDomainEventsEngine(services);
            RegisterDomainCommandsEngine(services);
            RegisterKLE(services, appSettings["KLEOnlineUrl"] ?? "https://api.kle-online.dk/resources/kle");
            RegisterOptions(services);
            RegisterBackgroundJobs(services);
            RegisterRoleAssignmentServices(services);
            RegisterMappers(services);
            RegisterLocalOptionTypes(services);
            RegisterGlobalOptionTypes(services);
        }

        private static void RegisterDataAccess(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("KitosContext")
                ?? throw new InvalidOperationException("KitosContext connection string is required");
            services.AddDbContext<KitosContext>((sp, options) =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                options.UseLazyLoadingProxies();
                // Include public in search_path so the citext extension type is discoverable.
                // HasDefaultSchema("dbo") causes Npgsql to set search_path=dbo on connect,
                // which would exclude public (where citext is installed) unless we override it here.
                var pgCsb = new NpgsqlConnectionStringBuilder(connectionString) { SearchPath = "dbo,public" };
                options.UseNpgsql(pgCsb.ConnectionString,
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "dbo"))
                    .ReplaceService<IMigrationsSqlGenerator, KitosNpgsqlMigrationsSqlGenerator>();

                options.AddInterceptors(new EFEntityInterceptor(
                        operationClock: () =>
                            httpContextAccessor.HttpContext?.RequestServices.GetService<IOperationClock>()
                            ?? new OperationClock(),
                        userContext: () =>
                            httpContextAccessor.HttpContext?.RequestServices.GetService<Maybe<ActiveUserIdContext>>()
                            ?? Maybe<ActiveUserIdContext>.None,
                        fallbackUserResolver: () =>
                            httpContextAccessor.HttpContext?.RequestServices.GetService<IFallbackUserResolver>()
                            ?? new BackgroundJobFallbackUserResolver(sp)));
            });

            services.AddScoped<ITransactionManager, TransactionManager>();

            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IItSystemRepository, ItSystemRepository>();
            services.AddScoped<IItSystemUsageRepository, ItSystemUsageRepository>();
            services.AddScoped<IItSystemUsageBatchLoadRepository, ItSystemUsageBatchLoadRepository>();
            services.AddScoped<IOrganizationUnitRepository, OrganizationUnitRepository>();
            services.AddScoped<IAdviceRepository, AdviceRepository>();
            services.AddScoped<IItContractRepository, ItContractRepository>();
            services.AddScoped<IInterfaceRepository, InterfaceRepository>();
            services.AddScoped<IKLEStandardRepository, KLEStandardRepository>();
            services.AddScoped<IKLEUpdateHistoryItemRepository, KLEUpdateHistoryItemRepository>();
            services.AddScoped<IBrokenExternalReferencesReportRepository, BrokenExternalReferencesReportRepository>();
            services.AddScoped<IReferenceRepository, ReferenceRepository>();
            services.AddScoped<IDataProcessingRegistrationRepository, DataProcessingRegistrationRepository>();
            services.AddScoped<IKendoOrganizationalConfigurationRepository, KendoOrganizationalConfigurationRepository>();
            services.AddScoped<IUserNotificationRepository, UserNotificationRepository>();
            services.AddScoped<ITaskRefRepository, TaskRefRepository>();
            services.AddScoped<IUIModuleCustomizationRepository, UIModuleCustomizationRepository>();
            services.AddScoped<ISsoUserIdentityRepository, SsoUserIdentityRepository>();
            services.AddScoped<IDatabaseControl, EntityFrameworkContextDatabaseControl>();
            services.AddScoped<IStsOrganizationIdentityRepository, StsOrganizationIdentityRepository>();
            services.AddScoped<IItSystemUsageAttachedOptionRepository, ItSystemUsageAttachedOptionRepository>();
            services.AddScoped<ISensitivePersonalDataTypeRepository, SensitivePersonalDataTypeRepository>();
            services.AddScoped<IDataProcessingRegistrationReadModelRepository, DataProcessingRegistrationReadModelRepository>();
            services.AddScoped<IPendingReadModelUpdateRepository, PendingReadModelUpdateRepository>();
            services.AddScoped<IDataProcessingRegistrationOptionRepository, DataProcessingRegistrationOptionRepository>();
            services.AddScoped<IItSystemUsageOverviewReadModelRepository, ItSystemUsageOverviewReadModelRepository>();
            services.AddScoped<IItContractOverviewReadModelRepository, ItContractOverviewReadModelRepository>();
            services.AddScoped<IItContractSupplierOverviewReadModelRepository, ItContractSupplierOverviewReadModelRepository>();

            services.AddScoped<IAuthorizationModelFactory, AuthorizationModelFactory>();
            services.AddScoped<IAuthorizationContextFactory, AuthorizationContextFactory>();
            services.AddScoped<IFieldAuthorizationModel, FieldAuthorizationModel>();
            services.AddScoped<UserContextFactory>();
            services.AddScoped<IUserContextFactory>(sp =>
                new CachingUserContextFactory(
                    sp.GetRequiredService<UserContextFactory>(),
                    sp.GetRequiredService<IObjectCache>()));
            services.AddScoped<IOrganizationalUserContext>(sp =>
            {
                var authentication = sp.GetRequiredService<IAuthenticationContext>();
                if (authentication.Method != AuthenticationMethod.Anonymous && authentication.UserId.HasValue)
                {
                    var userContextFactory = sp.GetRequiredService<IUserContextFactory>();
                    try
                    {
                        return userContextFactory.Create(authentication.UserId.Value);
                    }
                    catch (InvalidCastException ex) when (IsStalePostgreSqlTypeMap(ex))
                    {
                        ReloadPostgreSqlTypes(sp);
                        return userContextFactory.Create(authentication.UserId.Value);
                    }
                }
                return new UnauthenticatedUserContext();
            });
            services.AddScoped<IAuthorizationContext>(sp =>
                sp.GetRequiredService<IAuthorizationContextFactory>().Create(
                    sp.GetRequiredService<IOrganizationalUserContext>()));
        }

        private static bool IsStalePostgreSqlTypeMap(InvalidCastException exception)
        {
            return exception.Message.Contains("DataTypeName '-.-'", StringComparison.OrdinalIgnoreCase)
                   || exception.Message.Contains("cache lookup failed for type", StringComparison.OrdinalIgnoreCase);
        }

        private static void ReloadPostgreSqlTypes(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KitosContext>();
            if (!dbContext.Database.IsNpgsql())
            {
                return;
            }

            dbContext.Database.OpenConnection();
            try
            {
                if (dbContext.Database.GetDbConnection() is NpgsqlConnection npgsqlConnection)
                {
                    npgsqlConnection.ReloadTypes();
                    NpgsqlConnection.ClearAllPools();
                }
            }
            finally
            {
                dbContext.Database.CloseConnection();
            }
        }


        private static void RegisterDomainEventsEngine(IServiceCollection services)
        {
            services.AddScoped<IDomainEvents, NinjectDomainEventHandlerMediator>();

            RegisterDomainEvent<ClearCacheOnAdministrativeAccessRightsChangedHandler>(services);
            RegisterDomainEvent<UnbindBrokenReferenceReportsOnSourceDeletedHandler>(services);
            RegisterDomainEvent<CleanupDataProcessingRegistrationsOnSystemUsageDeletedEvent>(services);
            RegisterDomainEvent<BuildDataProcessingRegistrationReadModelOnChangesHandler>(services);
            RegisterDomainEvent<ResetDprMainContractWhenDprRemovedFromContractEventHandler>(services);
            RegisterDomainEvent<ContractDeletedSystemRelationsHandler>(services);
            RegisterDomainEvent<RelationSpecificInterfaceEventsHandler>(services);
            RegisterDomainEvent<UpdateRelationsOnSystemUsageDeletedHandler>(services);
            RegisterDomainEvent<ContractDeletedAdvicesHandler>(services);
            RegisterDomainEvent<DataProcessingRegistrationDeletedAdvicesHandler>(services);
            RegisterDomainEvent<SystemUsageDeletedAdvicesHandler>(services);
            RegisterDomainEvent<ContractDeletedUserNotificationsHandler>(services);
            RegisterDomainEvent<DataProcessingRegistrationDeletedUserNotificationsHandler>(services);
            RegisterDomainEvent<SystemUsageDeletedUserNotificationsHandler>(services);
            RegisterDomainEvent<BuildItSystemUsageOverviewReadModelOnChangesHandler>(services);
            RegisterDomainEvent<BuildItContractOverviewReadModelOnChangesHandler>(services);
            RegisterDomainEvent<BuildItContractSupplierOverviewReadModelOnChangesHandler>(services);
            RegisterDomainEvent<MarkEntityAsDirtyOnChangeEventHandler>(services);
            RegisterDomainEvent<TrackDeletedEntitiesEventHandler>(services);
            RegisterDomainEvent<HandleOrganizationBeingDeleted>(services);
            RegisterDomainEvent<SendEmailToStakeholdersOnExternalOrganizationConnectionUpdatedHandler>(services);
            RegisterDomainEvent<PublishSystemChangesEventHandler>(services);
            RegisterDomainEvent<PublishSystemTakenIntoUsageEventHandler>(services);
            RegisterDomainEvent<RaiseEntityUpdatedOnSnapshotEventsHandler<ItSystem, ItSystemSnapshot>>(services);
            RegisterDomainEvent<RaiseEntityUpdatedOnSnapshotEventsHandler<DataProcessingRegistration, DprSnapshot>>(services);
            RegisterDomainEvent<UserLoggedInEventHandler>(services);
        }

        private static void RegisterDomainEvent<THandler>(IServiceCollection services)
            where THandler : class
        {
            typeof(THandler)
                .GetInterfaces()
                .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>))
                .ToList()
                .ForEach(iface => services.AddScoped(iface, typeof(THandler)));
        }

        private static void RegisterDomainCommandsEngine(IServiceCollection services)
        {
            services.AddScoped<ICommandBus, NinjectCommandHandlerMediator>();

            RegisterCommand<RemoveUserFromOrganizationCommandHandler>(services);
            RegisterCommand<RemoveUserFromKitosCommandHandler>(services);
            RegisterCommand<RemoveOrganizationUnitRegistrationsCommandHandler>(services);
            RegisterCommand<AuthorizedUpdateOrganizationFromFKOrganisationCommandHandler>(services);
            RegisterCommand<ValidateUserCredentialsCommandHandler>(services);
            RegisterCommand<ReportPendingFkOrganizationChangesToStakeHoldersHandler>(services);
        }

        private static void RegisterCommand<THandler>(IServiceCollection services)
            where THandler : class
        {
            typeof(THandler)
                .GetInterfaces()
                .Where(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICommandHandler<,>))
                .ToList()
                .ForEach(iface => services.AddScoped(iface, typeof(THandler)));
        }

        private static void RegisterKLE(IServiceCollection services, string kleOnlineUrl)
        {
            services.AddScoped<IKLEApplicationService, KLEApplicationService>();
            services.AddScoped<IKLEDataBridge>(_ => new KLEDataBridge(kleOnlineUrl));
            services.AddScoped<IKLEParentHelper, KLEParentHelper>();
            services.AddScoped<IKLEConverterHelper, KLEConverterHelper>();
        }

        private static void RegisterOptions(IServiceCollection services)
        {
            // Open generic registrations for options application services
            services.AddScoped(typeof(IOptionsApplicationService<,>), typeof(OptionsApplicationService<,>));
            services.AddScoped(typeof(IRoleOptionsApplicationService<,>), typeof(RoleOptionsApplicationService<,>));

            // Attached options assignment services
            services.AddScoped<IAttachedOptionsAssignmentService<RegisterType, ItSystemUsage>>(sp =>
                new AttachedOptionsAssignmentService<RegisterType, ItSystemUsage>(
                    OptionType.REGISTERTYPEDATA,
                    sp.GetRequiredService<IItSystemUsageAttachedOptionRepository>(),
                    sp.GetRequiredService<IOptionsService<ItSystemUsage, RegisterType>>()));
            services.AddScoped<IAttachedOptionsAssignmentService<SensitivePersonalDataType, ItSystem>>(sp =>
                new AttachedOptionsAssignmentService<SensitivePersonalDataType, ItSystem>(
                    OptionType.SENSITIVEPERSONALDATA,
                    sp.GetRequiredService<IItSystemUsageAttachedOptionRepository>(),
                    sp.GetRequiredService<IOptionsService<ItSystem, SensitivePersonalDataType>>()));

            RegisterOptionsService<ItInterface, InterfaceType, LocalInterfaceType>(services);
            RegisterOptionsService<DataRow, DataType, LocalDataType>(services);
            RegisterOptionsService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, LocalDataProcessingRegistrationRole>(services);
            RegisterRoleOptionsService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, LocalDataProcessingRegistrationRole>(services);
            RegisterOptionsService<DataProcessingRegistration, DataProcessingCountryOption, LocalDataProcessingCountryOption>(services);
            RegisterOptionsService<DataProcessingRegistration, DataProcessingBasisForTransferOption, LocalDataProcessingBasisForTransferOption>(services);
            RegisterOptionsService<DataProcessingRegistration, DataProcessingDataResponsibleOption, LocalDataProcessingDataResponsibleOption>(services);
            RegisterOptionsService<DataProcessingRegistration, DataProcessingOversightOption, LocalDataProcessingOversightOption>(services);
            RegisterOptionsService<ItSystem, BusinessType, LocalBusinessType>(services);
            RegisterOptionsService<ItSystem, SensitivePersonalDataType, LocalSensitivePersonalDataType>(services);
            RegisterOptionsService<ItSystemRight, ItSystemRole, LocalItSystemRole>(services);
            RegisterRoleOptionsService<ItSystemRight, ItSystemRole, LocalItSystemRole>(services);
            RegisterOptionsService<SystemRelation, RelationFrequencyType, LocalRelationFrequencyType>(services);
            RegisterOptionsService<ItSystemUsage, ItSystemCategories, LocalItSystemCategories>(services);
            RegisterOptionsService<ItSystemUsage, ArchiveType, LocalArchiveType>(services);
            RegisterOptionsService<ItSystemUsage, ArchiveLocation, LocalArchiveLocation>(services);
            RegisterOptionsService<ItSystemUsage, ArchiveTestLocation, LocalArchiveTestLocation>(services);
            RegisterOptionsService<ItSystemUsage, SystemUsageCriticalityLevel, LocalSystemUsageCriticalityLevel>(services);
            RegisterOptionsService<ItSystemUsage, TechnicalSystemType, LocalTechnicalSystemType>(services);
            RegisterOptionsService<ItSystemUsage, RegisterType, LocalRegisterType>(services);
            RegisterOptionsService<ItContractRight, ItContractRole, LocalItContractRole>(services);
            RegisterRoleOptionsService<ItContractRight, ItContractRole, LocalItContractRole>(services);
            RegisterOptionsService<ItContract, ItContractType, LocalItContractType>(services);
            RegisterOptionsService<ItContract, ItContractTemplateType, LocalItContractTemplateType>(services);
            RegisterOptionsService<ItContract, PurchaseFormType, LocalPurchaseFormType>(services);
            RegisterOptionsService<ItContract, PaymentModelType, LocalPaymentModelType>(services);
            RegisterOptionsService<ItContract, AgreementElementType, LocalAgreementElementType>(services);
            RegisterOptionsService<ItContract, OptionExtendType, LocalOptionExtendType>(services);
            RegisterOptionsService<ItContract, PaymentFreqencyType, LocalPaymentFreqencyType>(services);
            RegisterOptionsService<ItContract, PriceRegulationType, LocalPriceRegulationType>(services);
            RegisterOptionsService<ItContract, ProcurementStrategyType, LocalProcurementStrategyType>(services);
            RegisterOptionsService<ItContract, TerminationDeadlineType, LocalTerminationDeadlineType>(services);
            RegisterOptionsService<ItContract, CriticalityType, LocalCriticalityType>(services);
            RegisterOptionsService<OrganizationUnitRight, OrganizationUnitRole, LocalOrganizationUnitRole>(services);
            RegisterRoleOptionsService<OrganizationUnitRight, OrganizationUnitRole, LocalOrganizationUnitRole>(services);
        }

        private static void RegisterOptionsService<TReferenceType, TOptionType, TLocalOptionType>(IServiceCollection services)
            where TLocalOptionType : LocalOptionEntity<TOptionType>, new()
            where TOptionType : OptionEntity<TReferenceType>
        {
            services.AddScoped<IOptionsService<TReferenceType, TOptionType>, OptionsService<TReferenceType, TOptionType, TLocalOptionType>>();
        }

        private static void RegisterRoleOptionsService<TReferenceType, TOptionType, TLocalOptionType>(IServiceCollection services)
            where TLocalOptionType : LocalRoleOptionEntity<TOptionType>, new()
            where TOptionType : OptionEntity<TReferenceType>, IRoleEntity
        {
            services.AddScoped<IRoleOptionsService<TReferenceType, TOptionType>, RoleOptionsService<TReferenceType, TOptionType, TLocalOptionType>>();
        }

        private static void RegisterBackgroundJobs(IServiceCollection services)
        {
            services.AddScoped<IBackgroundJobLauncher, BackgroundJobLauncher>();
            services.AddScoped<IBackgroundJobScheduler, BackgroundJobScheduler>();
            services.AddScoped<IRebuildReadModelsJobFactory, RebuildReadModelsJobFactory>();

            // Background job self-registrations (resolved directly by type from Hangfire)
            services.AddScoped<CheckExternalLinksBackgroundJob>();
            services.AddScoped<RebuildDataProcessingRegistrationReadModelsBatchJob>();
            services.AddScoped<ScheduleDataProcessingRegistrationReadModelUpdates>();
            services.AddScoped<RebuildItSystemUsageOverviewReadModelsBatchJob>();
            services.AddScoped<ScheduleItSystemUsageOverviewReadModelUpdates>();
            services.AddScoped<ScheduleUpdatesForItSystemUsageReadModelsWhichChangesActiveState>();
            services.AddScoped<ScheduleUpdatesForItContractOverviewReadModelsWhichChangesActiveState>();
            services.AddScoped<ScheduleItContractOverviewReadModelUpdates>();
            services.AddScoped<ScheduleItContractSupplierOverviewReadModelUpdates>();
            services.AddScoped<ScheduleUpdatesForDataProcessingRegistrationOverviewReadModelsWhichChangesActiveState>();
            services.AddScoped<RebuildItContractOverviewReadModelsBatchJob>();
            services.AddScoped<RebuildItContractSupplierOverviewReadModelsBatchJob>();
            services.AddScoped<PurgeDuplicatePendingReadModelUpdates>();
            services.AddScoped<PurgeOrphanedHangfireJobs>();
            services.AddScoped<ScheduleFkOrgUpdatesBackgroundJob>();
            services.AddScoped<CreateInitialPublicMessages>();
            services.AddScoped<CreateMainPublicMessage>();
        }

        private static void RegisterRoleAssignmentServices(IServiceCollection services)
        {
            RegisterRoleAssignmentService<ItSystemRight, ItSystemRole, ItSystemUsage>(services);
            RegisterRoleAssignmentService<DataProcessingRegistrationRight, DataProcessingRegistrationRole, DataProcessingRegistration>(services);
            RegisterRoleAssignmentService<ItContractRight, ItContractRole, ItContract>(services);
            RegisterRoleAssignmentService<OrganizationUnitRight, OrganizationUnitRole, OrganizationUnit>(services);
        }

        private static void RegisterRoleAssignmentService<TRight, TRole, TEntity>(IServiceCollection services)
            where TRight : Entity, IRight<TEntity, TRight, TRole>
            where TRole : OptionEntity<TRight>, IRoleEntity, IOptionReference<TRight>, new()
            where TEntity : HasRightsEntity<TEntity, TRight, TRole>, IOwnedByOrganization
        {
            services.AddScoped<IRoleAssignmentService<TRight, TRole, TEntity>, RoleAssignmentService<TRight, TRole, TEntity>>();
        }

        private static void RegisterMappers(IServiceCollection services)
        {
            services.AddScoped<IEntityWithDeactivatedStatusMapper, EntityWithDeactivatedStatusMapper>();
            services.AddScoped<ILocalOptionTypeResponseMapper, LocalOptionTypeResponseMapper>();
            services.AddScoped<ILocalOptionTypeWriteModelMapper, LocalOptionTypeWriteModelMapper>();
            services.AddScoped<IItSystemWriteModelMapper, ItSystemWriteModelMapper>();
            services.AddScoped<IItSystemResponseMapper, ItSystemResponseMapper>();
            services.AddScoped<IItSystemUsageResponseMapper, ItSystemUsageResponseMapper>();
            services.AddScoped<IItSystemUsageWriteModelMapper, ItSystemUsageWriteModelMapper>();
            services.AddScoped<IItSystemUsageMigrationResponseMapper, ItSystemUsageMigrationResponseMapper>();
            services.AddScoped<IItSystemUsageArchiveResponseMapper, ItSystemUsageArchiveResponseMapper>();
            services.AddScoped<IDataProcessingRegistrationWriteModelMapper, DataProcessingRegistrationWriteModelMapper>();
            services.AddScoped<IDataProcessingRegistrationResponseMapper, DataProcessingRegistrationResponseMapper>();
            services.AddScoped<IItContractWriteModelMapper, ItContractWriteModelMapper>();
            services.AddScoped<IItContractResponseMapper, ItContractResponseMapper>();
            services.AddScoped<IItInterfaceWriteModelMapper, ItInterfaceWriteModelMapper>();
            services.AddScoped<IItInterfaceResponseMapper, ItInterfaceResponseMapper>();
            services.AddScoped<IExternalReferenceResponseMapper, ExternalReferenceResponseMapper>();
            services.AddScoped<IResourcePermissionsResponseMapper, ResourcePermissionsResponseMapper>();
            services.AddScoped<ICommandPermissionsResponseMapper, CommandPermissionsResponseMapper>();
            services.AddScoped<IPublicMessagesWriteModelMapper, PublicMessagesWriteModelMapper>();
            services.AddScoped<INotificationWriteModelMapper, NotificationWriteModelMapper>();
            services.AddScoped<INotificationResponseMapper, NotificationResponseMapper>();
            services.AddScoped<IOrganizationUnitWriteModelMapper, OrganizationUnitWriteModelMapper>();
            services.AddScoped<IOrganizationUnitResponseModelMapper, OrganizationUnitResponseModelMapper>();
            services.AddScoped<IUserWriteModelMapper, UserWriteModelMapper>();
            services.AddScoped<IUserResponseModelMapper, UserResponseModelMapper>();
            services.AddScoped<IOrganizationResponseMapper, OrganizationResponseMapper>();
            services.AddScoped<IOrganizationWriteModelMapper, OrganizationWriteModelMapper>();
            services.AddScoped<IOrganizationTypeMapper, OrganizationTypeMapper>();
            services.AddScoped<IGlobalOptionTypeWriteModelMapper, GlobalOptionTypeWriteModelMapper>();
            services.AddScoped<IGlobalOptionTypeResponseMapper, GlobalOptionTypeResponseMapper>();
            services.AddScoped<IHelpTextResponseMapper, HelpTextResponseMapper>();
            services.AddScoped<IHelpTextWriteModelMapper, HelpTextWriteModelMapper>();
            services.AddScoped<ILegalPropertyWriteModelMapper, LegalPropertyWriteModelMapper>();
        }

        private static void RegisterLocalOptionTypes(IServiceCollection services)
        {
            RegisterLocalItSystemOptionTypes(services);
            RegisterLocalDprOptionTypes(services);
            RegisterLocalItContractOptionTypes(services);
            RegisterLocalRoleOptionService<LocalOrganizationUnitRole, OrganizationUnitRight, OrganizationUnitRole>(services);
        }

        private static void RegisterGlobalOptionTypes(IServiceCollection services)
        {
            RegisterGlobalRegularOptionService<BusinessType, ItSystem>(services);
            RegisterGlobalRegularOptionService<ArchiveLocation, ItSystemUsage>(services);
            RegisterGlobalRegularOptionService<SystemUsageCriticalityLevel, ItSystemUsage>(services);
            RegisterGlobalRegularOptionService<TechnicalSystemType, ItSystemUsage>(services);
            RegisterGlobalRoleOptionService<ItSystemRole, ItSystemRight>(services);
            RegisterGlobalRegularOptionService<SensitivePersonalDataType, ItSystem>(services);
            RegisterGlobalRegularOptionService<ItSystemRole, ItSystemRight>(services);
            RegisterGlobalRegularOptionService<RegisterType, ItSystemUsage>(services);
            RegisterGlobalRegularOptionService<ItSystemCategories, ItSystemUsage>(services);
            RegisterGlobalRegularOptionService<InterfaceType, ItInterface>(services);
            RegisterGlobalRegularOptionService<RelationFrequencyType, SystemRelation>(services);
            RegisterGlobalRegularOptionService<DataType, DataRow>(services);
            RegisterGlobalRegularOptionService<ArchiveType, ItSystemUsage>(services);
            RegisterGlobalRegularOptionService<ArchiveTestLocation, ItSystemUsage>(services);
            RegisterGlobalRegularOptionService<OptionExtendType, ItContract>(services);
            RegisterGlobalRegularOptionService<TerminationDeadlineType, ItContract>(services);
            RegisterGlobalRegularOptionService<PurchaseFormType, ItContract>(services);
            RegisterGlobalRegularOptionService<ProcurementStrategyType, ItContract>(services);
            RegisterGlobalRegularOptionService<PriceRegulationType, ItContract>(services);
            RegisterGlobalRegularOptionService<PaymentModelType, ItContract>(services);
            RegisterGlobalRegularOptionService<PaymentFreqencyType, ItContract>(services);
            RegisterGlobalRegularOptionService<ItContractType, ItContract>(services);
            RegisterGlobalRegularOptionService<ItContractTemplateType, ItContract>(services);
            RegisterGlobalRoleOptionService<ItContractRole, ItContractRight>(services);
            RegisterGlobalRegularOptionService<CriticalityType, ItContract>(services);
            RegisterGlobalRegularOptionService<AgreementElementType, ItContract>(services);
            RegisterGlobalRoleOptionService<DataProcessingRegistrationRole, DataProcessingRegistrationRight>(services);
            RegisterGlobalRegularOptionService<DataProcessingCountryOption, DataProcessingRegistration>(services);
            RegisterGlobalRegularOptionService<DataProcessingOversightOption, DataProcessingRegistration>(services);
            RegisterGlobalRegularOptionService<DataProcessingDataResponsibleOption, DataProcessingRegistration>(services);
            RegisterGlobalRegularOptionService<DataProcessingBasisForTransferOption, DataProcessingRegistration>(services);
            RegisterGlobalRegularOptionService<CountryCode, Organization>(services);
            RegisterGlobalRoleOptionService<OrganizationUnitRole, OrganizationUnitRight>(services);
        }

        private static void RegisterLocalItContractOptionTypes(IServiceCollection services)
        {
            RegisterLocalRoleOptionService<LocalItContractRole, ItContractRight, ItContractRole>(services);
            RegisterLocalOptionService<LocalItContractType, ItContract, ItContractType>(services);
            RegisterLocalOptionService<LocalItContractTemplateType, ItContract, ItContractTemplateType>(services);
            RegisterLocalOptionService<LocalPurchaseFormType, ItContract, PurchaseFormType>(services);
            RegisterLocalOptionService<LocalPaymentModelType, ItContract, PaymentModelType>(services);
            RegisterLocalOptionService<LocalAgreementElementType, ItContract, AgreementElementType>(services);
            RegisterLocalOptionService<LocalOptionExtendType, ItContract, OptionExtendType>(services);
            RegisterLocalOptionService<LocalPaymentFreqencyType, ItContract, PaymentFreqencyType>(services);
            RegisterLocalOptionService<LocalPriceRegulationType, ItContract, PriceRegulationType>(services);
            RegisterLocalOptionService<LocalProcurementStrategyType, ItContract, ProcurementStrategyType>(services);
            RegisterLocalOptionService<LocalTerminationDeadlineType, ItContract, TerminationDeadlineType>(services);
            RegisterLocalOptionService<LocalCriticalityType, ItContract, CriticalityType>(services);
        }

        private static void RegisterLocalItSystemOptionTypes(IServiceCollection services)
        {
            RegisterLocalRoleOptionService<LocalItSystemRole, ItSystemRight, ItSystemRole>(services);
            RegisterLocalOptionService<LocalBusinessType, ItSystem, BusinessType>(services);
            RegisterLocalOptionService<LocalArchiveType, ItSystemUsage, ArchiveType>(services);
            RegisterLocalOptionService<LocalArchiveLocation, ItSystemUsage, ArchiveLocation>(services);
            RegisterLocalOptionService<LocalArchiveTestLocation, ItSystemUsage, ArchiveTestLocation>(services);
            RegisterLocalOptionService<LocalSystemUsageCriticalityLevel, ItSystemUsage, SystemUsageCriticalityLevel>(services);
            RegisterLocalOptionService<LocalTechnicalSystemType, ItSystemUsage, TechnicalSystemType>(services);
            RegisterLocalOptionService<LocalDataType, DataRow, DataType>(services);
            RegisterLocalOptionService<LocalRelationFrequencyType, SystemRelation, RelationFrequencyType>(services);
            RegisterLocalOptionService<LocalInterfaceType, ItInterface, InterfaceType>(services);
            RegisterLocalOptionService<LocalSensitivePersonalDataType, ItSystem, SensitivePersonalDataType>(services);
            RegisterLocalOptionService<LocalItSystemCategories, ItSystemUsage, ItSystemCategories>(services);
            RegisterLocalOptionService<LocalRegisterType, ItSystemUsage, RegisterType>(services);
        }

        private static void RegisterLocalDprOptionTypes(IServiceCollection services)
        {
            RegisterLocalRoleOptionService<LocalDataProcessingRegistrationRole, DataProcessingRegistrationRight, DataProcessingRegistrationRole>(services);
            RegisterLocalOptionService<LocalDataProcessingBasisForTransferOption, DataProcessingRegistration, DataProcessingBasisForTransferOption>(services);
            RegisterLocalOptionService<LocalDataProcessingOversightOption, DataProcessingRegistration, DataProcessingOversightOption>(services);
            RegisterLocalOptionService<LocalDataProcessingDataResponsibleOption, DataProcessingRegistration, DataProcessingDataResponsibleOption>(services);
            RegisterLocalOptionService<LocalDataProcessingCountryOption, DataProcessingRegistration, DataProcessingCountryOption>(services);
        }

        private static void RegisterLocalOptionService<TLocalOptionType, TReferenceType, TOptionType>(IServiceCollection services)
            where TLocalOptionType : LocalOptionEntity<TOptionType>, new()
            where TOptionType : OptionEntity<TReferenceType>
        {
            services.AddScoped<IGenericLocalOptionsService<TLocalOptionType, TReferenceType, TOptionType>,
                GenericLocalOptionsService<TLocalOptionType, TReferenceType, TOptionType>>();
        }

        private static void RegisterLocalRoleOptionService<TLocalOptionType, TReferenceType, TOptionType>(IServiceCollection services)
            where TLocalOptionType : LocalRoleOptionEntity<TOptionType>, new()
            where TOptionType : OptionEntity<TReferenceType>
        {
            services.AddScoped<IGenericLocalRoleOptionsService<TLocalOptionType, TReferenceType, TOptionType>,
                GenericLocalRoleOptionsService<TLocalOptionType, TReferenceType, TOptionType>>();
        }

        private static void RegisterGlobalRegularOptionService<TOptionType, TReferenceType>(IServiceCollection services)
            where TOptionType : OptionEntity<TReferenceType>, new()
        {
            services.AddScoped<IGlobalRegularOptionsService<TOptionType, TReferenceType>,
                GlobalRegularOptionsService<TOptionType, TReferenceType>>();
        }

        private static void RegisterGlobalRoleOptionService<TOptionType, TReferenceType>(IServiceCollection services)
            where TOptionType : OptionEntity<TReferenceType>, IRoleEntity, new()
        {
            services.AddScoped<IGlobalRoleOptionsService<TOptionType, TReferenceType>,
                GlobalRoleOptionsService<TOptionType, TReferenceType>>();
        }
    }
}
