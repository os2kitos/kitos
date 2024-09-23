using System;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Microsoft.OData.Edm;
using Presentation.Web.Controllers.API.V1.OData;
using Presentation.Web.Controllers.API.V1.OData.LocalOptionControllers;
using Core.DomainModel.LocalOptions;
using Presentation.Web.Controllers.API.V1.OData.OptionControllers;
using Presentation.Web.Infrastructure;
using Core.DomainModel.Advice;
using System.Linq;
using Presentation.Web.Controllers.API.V1.OData.AttachedOptions;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using System.Collections.Generic;
using Core.DomainModel.GDPR;
using Core.DomainModel.GDPR.Read;
using Core.DomainModel.ItContract.Read;
using Core.DomainModel.ItSystemUsage.Read;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing.Conventions;
using Presentation.Web.Infrastructure.Attributes;
using DataType = Core.DomainModel.ItSystem.DataType;
using HelpText = Core.DomainModel.HelpText;
using Core.DomainModel.Shared;

namespace Presentation.Web
{
    public static class WebApiConfig
    {
        private const string ControllerSuffix = "Controller";

        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();
            const string routeName = "odata";
            const string routePrefix = "odata";

            var route = config.MapODataServiceRoute(routeName: routeName, routePrefix: routePrefix, configureAction: (builder => builder
            .AddService(ServiceLifetime.Singleton, sp => GetModel())
            .AddService<ODataUriResolver>(ServiceLifetime.Singleton, sp => new UnqualifiedCallAndEnumPrefixFreeResolver { EnableCaseInsensitive = true })
            .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp => ODataRoutingConventions.CreateDefaultWithAttributeRouting(routeName, config))));

            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Filters.Add(new ExceptionLogFilterAttribute());
            config.Filters.Add(new RequireValidatedCSRFAttributed());
            config.Filters.Add(new ValidateActionParametersAttribute());
            config.Filters.Add(new DenyRightsHoldersAccessAttribute()); //By default block all actions for users with rights holders access in one or more organizations
            config.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
        }

        public static IEdmModel GetModel()
        {
            const string kitosNamespace = "Kitos";
            const string entitySetOrganizations = "Organizations";
            const string entitySetItSystems = "ItSystems";

            var builder = new ODataConventionModelBuilder();

            // BUG with EnableLowerCamelCase http://stackoverflow.com/questions/39269261/odata-complains-about-missing-id-property-when-enabling-camelcasing
            //builder.EnableLowerCamelCase();

            var accessMod = builder.AddEnumType(typeof(AccessModifier));

            accessMod.Namespace = kitosNamespace;
            var orgRoles = builder.AddEnumType(typeof(OrganizationRole));
            orgRoles.Namespace = kitosNamespace;
            var objectTypes = builder.AddEnumType(typeof(RelatedEntityType));
            objectTypes.Namespace = kitosNamespace;
            var schedulings = builder.AddEnumType(typeof(Scheduling));
            schedulings.Namespace = kitosNamespace;
            var optionsTypes = builder.AddEnumType(typeof(OptionType));
            optionsTypes.Namespace = kitosNamespace;

            var organizationRights = BindEntitySet<OrganizationRight, OrganizationRightsController>(builder);
            organizationRights.HasRequiredBinding(o => o.Organization, entitySetOrganizations);

            BindEntitySet<AgreementElementType, AgreementElementTypesController>(builder);

            var itContractAgreementElementTypes = builder.EntitySet<ItContractAgreementElementTypes>("ItContractAgreementElementTypes");
            itContractAgreementElementTypes.EntityType.HasKey(x => x.ItContract_Id).HasKey(x => x.AgreementElementType_Id);

            BindEntitySet<ItContractTemplateType, ItContractTemplateTypesController>(builder);

            BindEntitySet<ItContractType, ItContractTypesController>(builder);

            BindEntitySet<RelationFrequencyType, FrequencyTypesController>(builder);

            BindEntitySet<ItContractRight, ItContractRightsController>(builder);

            BindEntitySet<ItContractRole, ItContractRolesController>(builder);

            BindEntitySet<AttachedOption, AttachedOptionsController>(builder);

            BindEntitySet<DataType, DataTypesController>(builder);

            var dataRow = builder.EntitySet<DataRow>("DataRows"); // no controller yet
            dataRow.EntityType.HasKey(x => x.Id);

            BindEntitySet<ArchiveLocation, ArchiveLocationsController>(builder);

            BindEntitySet<ArchiveTestLocation, ArchiveTestLocationsController>(builder);

            BindEntitySet<ArchiveType, ArchiveTypesController>(builder);

            BindEntitySet<ItSystemCategories, ItSystemCategoriesController>(builder);

            var itSystems = BindEntitySet<ItSystem, ItSystemsController>(builder);
            itSystems.HasRequiredBinding(o => o.Organization, entitySetOrganizations);
            itSystems.HasRequiredBinding(o => o.BelongsTo, entitySetOrganizations);
            itSystems.HasManyBinding(i => i.Children, entitySetItSystems);
            itSystems.HasRequiredBinding(i => i.Parent, entitySetItSystems);

            var businessTypes = BindEntitySet<BusinessType, BusinessTypesController>(builder);
            businessTypes.HasManyBinding(b => b.References, entitySetItSystems);

            var taskRefs = builder.EntitySet<TaskRef>("TaskRefs"); // no controller yet
            taskRefs.HasManyBinding(t => t.ItSystems, entitySetItSystems);
            taskRefs.EntityType.HasKey(x => x.Id);

            var orgNameSpace = entitySetOrganizations;

            var organizations = BindEntitySet<Organization, OrganizationsController>(builder);
            builder.StructuralTypes.First(t => t.ClrType == typeof(Organization)).RemoveProperty(typeof(Organization).GetProperty(nameof(Organization.IsDefaultOrganization)));

            organizations.EntityType.HasMany(x => x.OrgUnits).IsNavigable().Name = "OrganizationUnits";
            organizations.EntityType.Property(p => p.Uuid).IsOptional();

            organizations.HasManyBinding(o => o.ItSystems, entitySetItSystems);
            organizations.HasManyBinding(o => o.BelongingSystems, entitySetItSystems);

            var removeUserAction = organizations.EntityType.Collection.Action("RemoveUser");
            removeUserAction.Parameter<int>("orgKey").Required();
            removeUserAction.Parameter<int>("userId").Required();
            removeUserAction.Namespace = orgNameSpace;

            var orgUnits = builder.EntitySet<OrganizationUnit>(nameof(OrganizationUnitsController).Replace(ControllerSuffix, string.Empty));
            orgUnits.HasRequiredBinding(o => o.Organization, entitySetOrganizations);
            orgUnits.EntityType.HasKey(x => x.Id);
            orgUnits.EntityType.HasMany(x => x.ResponsibleForItContracts).Name = "ItContracts";
            //Add isActive to result form odata
            builder.StructuralTypes.First(t => t.ClrType == typeof(ItContract)).AddProperty(typeof(ItContract).GetProperty(nameof(ItContract.IsActive)));

            var userNameSpace = "Users";
            var userEntitySetName = nameof(UsersController).Replace(ControllerSuffix, string.Empty);
            var users = builder.EntitySet<User>(userEntitySetName);
            users.EntityType.HasKey(x => x.Id);
            users.EntityType.Ignore(x => x.Password);
            users.EntityType.Ignore(x => x.Salt);
            users.EntityType.Property(x => x.Name).IsRequired();
            users.EntityType.Property(x => x.Email).IsRequired();

            var orgGetUsersFunction = organizations.EntityType.Function("GetUsers").ReturnsCollectionFromEntitySet<User>(userEntitySetName);
            orgGetUsersFunction.Namespace = orgNameSpace;

            var userCreateAction = users.EntityType.Collection.Action("Create").ReturnsFromEntitySet<User>(userEntitySetName);
            userCreateAction.Namespace = userNameSpace;
            userCreateAction.Parameter<User>("user").Required();
            userCreateAction.Parameter<int>("organizationId").Required();
            userCreateAction.Parameter<bool>("sendMailOnCreation").Optional();

            var userCheckEmailFunction = users.EntityType.Collection.Function("IsEmailAvailable").Returns<bool>();
            userCheckEmailFunction.Parameter<string>("email").Required();
            userCheckEmailFunction.Namespace = userNameSpace;

            var userGetByMailFunction = builder.Function("GetUserByEmail").ReturnsFromEntitySet<User>(userEntitySetName);
            userGetByMailFunction.Parameter<string>("email").Required();

            var userGetByMailAndOrgFunction = builder.Function("GetUserByEmailAndOrganizationRelationship").ReturnsFromEntitySet<User>(userEntitySetName);
            userGetByMailAndOrgFunction.Parameter<string>("email").Required();
            userGetByMailAndOrgFunction.Parameter<int>("organizationId").Required();

            var getUsersByOrgUuidFunction = builder.Function("GetUsersByUuid")
                .ReturnsCollectionFromEntitySet<User>(userEntitySetName);
            getUsersByOrgUuidFunction.Parameter<Guid>("organizationUuid").Required();

            BindItSystemUsage(builder, entitySetOrganizations, entitySetItSystems);

            var contracts = BindEntitySet<ItContract, ItContractsController>(builder);
            contracts.HasRequiredBinding(o => o.Organization, entitySetOrganizations);
            contracts.HasRequiredBinding(o => o.Supplier, entitySetOrganizations);

            //contract read models
            BindEntitySet<ItContractOverviewReadModel, ItContractOverviewReadModelsController>(builder);
            builder.StructuralTypes.First(t => t.ClrType == typeof(ItContractOverviewReadModel)).RemoveProperty(typeof(ItContractOverviewReadModel).GetProperty(nameof(ItContractOverviewReadModel.SourceEntity)));
            builder.StructuralTypes.First(t => t.ClrType == typeof(ItContractOverviewReadModel)).RemoveProperty(typeof(ItContractOverviewReadModel).GetProperty(nameof(ItContractOverviewReadModel.Organization)));

            BindEntitySet<InterfaceType, InterfaceTypesController>(builder);

            var itInterfaces = BindEntitySet<ItInterface, ItInterfacesController>(builder);
            itInterfaces.HasRequiredBinding(o => o.Organization, entitySetOrganizations);

            var itInterfaceExihibits = builder.EntitySet<ItInterfaceExhibit>("ItInterfaceExhibits"); // no controller yet
            itInterfaceExihibits.HasRequiredBinding(o => o.ItSystem, entitySetItSystems);
            itInterfaceExihibits.EntityType.HasKey(x => x.Id);

            BindEntitySet<SensitiveDataType, SensitiveDataTypesController>(builder);

            BindEntitySet<RegisterType, RegisterTypesController>(builder);

            var sensitivePersonalDataTypes = BindEntitySet<SensitivePersonalDataType, SensitivePersonalDataTypesController>(builder);
            sensitivePersonalDataTypes.HasManyBinding(b => b.References, entitySetItSystems);

            BindEntitySet<OptionExtendType, OptionExtendTypesController>(builder);

            BindEntitySet<OrganizationUnitRight, OrganizationUnitRightsController>(builder);

            BindEntitySet<OrganizationUnitRole, OrganizationUnitRolesController>(builder);

            BindEntitySet<PaymentFreqencyType, PaymentFrequencyTypesController>(builder);

            BindEntitySet<PaymentModelType, PaymentModelTypesController>(builder);

            BindEntitySet<PriceRegulationType, PriceRegulationTypesController>(builder);

            BindEntitySet<ProcurementStrategyType, ProcurementStrategyTypesController>(builder);

            BindEntitySet<PurchaseFormType, PurchaseFormTypesController>(builder);

            BindEntitySet<CriticalityType, CriticalityTypesController>(builder);

            //Local options

            var localAgreementElementType = BindEntitySet<LocalAgreementElementType, LocalAgreementElementTypesController>(builder);
            localAgreementElementType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);


            var localArchiveType = BindEntitySet<LocalArchiveType, LocalArchiveTypesController>(builder);
            localArchiveType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localArchiveLocation = BindEntitySet<LocalArchiveLocation, LocalArchiveLocationsController>(builder);
            localArchiveLocation.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localArchiveTestLocation = BindEntitySet<LocalArchiveTestLocation, LocalArchiveTestLocationsController>(builder);
            localArchiveTestLocation.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localItSystemCategories = BindEntitySet<LocalItSystemCategories, LocalItSystemCategoriesController>(builder);
            localItSystemCategories.HasRequiredBinding(x => x.Organization, entitySetOrganizations);

            var localBusinessType = BindEntitySet<LocalBusinessType, LocalBusinessTypesController>(builder);
            localBusinessType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localDataType = BindEntitySet<LocalDataType, LocalDataTypesController>(builder);
            localDataType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localFrequencyType = BindEntitySet<LocalRelationFrequencyType, LocalFrequencyTypesController>(builder);
            localFrequencyType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localInterfaceType = BindEntitySet<LocalInterfaceType, LocalInterfaceTypesController>(builder);
            localInterfaceType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localItContractRole = BindEntitySet<LocalItContractRole, LocalItContractRolesController>(builder);
            localItContractRole.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localItContractTemplateType = BindEntitySet<LocalItContractTemplateType, LocalItContractTemplateTypesController>(builder);
            localItContractTemplateType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localItContractType = BindEntitySet<LocalItContractType, LocalItContractTypesController>(builder);
            localItContractType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localItSystemRole = BindEntitySet<LocalItSystemRole, LocalItSystemRolesController>(builder);
            localItSystemRole.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localOptionExtendType = BindEntitySet<LocalOptionExtendType, LocalOptionExtendTypesController>(builder);
            localOptionExtendType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localPaymentFreqencyType = BindEntitySet<LocalPaymentFreqencyType, LocalPaymentFrequencyTypesController>(builder);
            localPaymentFreqencyType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localPaymentModelType = BindEntitySet<LocalPaymentModelType, LocalPaymentModelTypesController>(builder);
            localPaymentModelType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localPriceRegulationType = BindEntitySet<LocalPriceRegulationType, LocalPriceRegulationTypesController>(builder);
            localPriceRegulationType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localProcurementStrategyType = BindEntitySet<LocalProcurementStrategyType, LocalProcurementStrategyTypesController>(builder);
            localProcurementStrategyType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localPurchaseFormType = BindEntitySet<LocalPurchaseFormType, LocalPurchaseFormTypesController>(builder);
            localPurchaseFormType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localCriticalityType = BindEntitySet<LocalCriticalityType, LocalCriticalityTypesController>(builder);
            localCriticalityType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var removeOption = builder.Function("RemoveOption");
            removeOption.Parameter<int>("id");
            removeOption.Parameter<int>("objectId");
            removeOption.Parameter<int>("type");
            removeOption.Parameter<int>("entityType");
            removeOption.Returns<IHttpActionResult>();

            var getSensitivePersonalDataByUsageId = builder.Function("GetSensitivePersonalDataByUsageId");
            getSensitivePersonalDataByUsageId.Parameter<int>("id");
            getSensitivePersonalDataByUsageId.ReturnsCollectionFromEntitySet<SensitivePersonalDataType>("SensitivePersonalDataTypes");
            builder.StructuralTypes.First(t => t.ClrType == typeof(SensitivePersonalDataType)).AddProperty(typeof(SensitivePersonalDataType).GetProperty(nameof(SensitivePersonalDataType.Checked)));
            getSensitivePersonalDataByUsageId.Namespace = "gdpr";

            var getRegisterTypeByObjectId = builder.Function("GetRegisterTypesByObjectID");
            getRegisterTypeByObjectId.Parameter<int>("id");
            getRegisterTypeByObjectId.ReturnsCollectionFromEntitySet<RegisterType>("RegisterTypes");
            builder.StructuralTypes.First(t => t.ClrType == typeof(RegisterType)).AddProperty(typeof(RegisterType).GetProperty(nameof(SensitivePersonalDataType.Checked)));

            var localSensitiveDataType = BindEntitySet<LocalSensitiveDataType, LocalSensitiveDataTypesController>(builder);
            localSensitiveDataType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localSensitivePersonalDataTypes = BindEntitySet<LocalSensitivePersonalDataType, LocalSensitivePersonalDataTypesController>(builder);
            localSensitivePersonalDataTypes.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localRegisterTypes = BindEntitySet<LocalRegisterType, LocalRegisterTypesController>(builder);
            localRegisterTypes.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localTerminationDeadlineType = BindEntitySet<LocalTerminationDeadlineType, LocalTerminationDeadlineTypesController>(builder);
            localTerminationDeadlineType.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var localOrganizationUnitRole = BindEntitySet<LocalOrganizationUnitRole, LocalOrganizationUnitRolesController>(builder);
            localOrganizationUnitRole.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            BindEntitySet<TerminationDeadlineType, TerminationDeadlineTypesController>(builder);

            var config = BindEntitySet<Config, ConfigsController>(builder);
            config.HasRequiredBinding(u => u.Organization, entitySetOrganizations);

            var getAdvicesByOrg = builder.Function("GetAdvicesByOrganizationId");
            getAdvicesByOrg.Parameter<int>("organizationId").Required();
            getAdvicesByOrg.ReturnsCollectionFromEntitySet<Advice>("Advice");

            var deactivateAdvice = builder.Function("DeactivateAdvice");
            deactivateAdvice.Parameter<int>("key").Required();
            deactivateAdvice.Returns<IHttpActionResult>();

            BindEntitySet<Advice, AdviceController>(builder);
            builder.StructuralTypes.First(t => t.ClrType == typeof(Advice)).AddProperty(typeof(Advice).GetProperty(nameof(Advice.CanBeDeleted)));

            BindEntitySet<AdviceSent, AdviceSentController>(builder);

            BindEntitySet<ArchivePeriod, ArchivePeriodsController>(builder);

            var references = builder.EntitySet<ExternalReference>("ExternalReferences");
            references.EntityType.HasKey(x => x.Id);
            references.HasRequiredBinding(a => a.ItSystem, entitySetItSystems);

            BindEntitySet<HelpText, HelpTextsController>(builder);

            BindDataProcessingRegistrationModule(builder);

            return builder.GetEdmModel();
        }

        private static void BindItSystemUsage(ODataConventionModelBuilder builder, string entitySetOrganizations, string entitySetItSystems)
        {
            var usages = BindEntitySet<ItSystemUsage, ItSystemUsagesController>(builder);
            usages.HasRequiredBinding(u => u.Organization, entitySetOrganizations);
            usages.HasRequiredBinding(u => u.ItSystem, entitySetItSystems);

            var itSystemRights = BindEntitySet<ItSystemRight, ItSystemRightsController>(builder);
            itSystemRights.HasRequiredBinding(u => u.Role, "ItSystemRoles");

            BindEntitySet<ItSystemRole, ItSystemRolesController>(builder);

            var systemOrgUnitUsages =
                builder.EntitySet<ItSystemUsageOrgUnitUsage>("ItSystemUsageOrgUnitUsages"); // no controller yet
            systemOrgUnitUsages.EntityType.HasKey(x => x.ItSystemUsageId).HasKey(x => x.OrganizationUnitId);

            var contractItSystemUsages =
                builder.EntitySet<ItContractItSystemUsage>("ItContractItSystemUsages"); // no controller yet
            contractItSystemUsages.EntityType.HasKey(x => x.ItContractId).HasKey(x => x.ItSystemUsageId);
            builder.StructuralTypes.First(t => t.ClrType == typeof(ItSystemUsage))
                .AddProperty(typeof(ItSystemUsage).GetProperty(nameof(ItSystemUsage.IsActiveAccordingToDateFields)));

            //read models
            BindEntitySet<ItSystemUsageOverviewReadModel, ItSystemUsageOverviewReadModelsController>(builder);
            builder.StructuralTypes.First(t => t.ClrType == typeof(ItSystemUsageOverviewReadModel)).RemoveProperty(typeof(ItSystemUsageOverviewReadModel).GetProperty(nameof(ItSystemUsageOverviewReadModel.SourceEntity)));
            builder.StructuralTypes.First(t => t.ClrType == typeof(ItSystemUsageOverviewReadModel)).RemoveProperty(typeof(ItSystemUsageOverviewReadModel).GetProperty(nameof(ItSystemUsageOverviewReadModel.Organization)));

            //Add ActiveArchivePeriodEndDate to result form odata
            builder.StructuralTypes.First(t => t.ClrType == typeof(ItSystemUsageOverviewReadModel)).AddProperty(typeof(ItSystemUsageOverviewReadModel).GetProperty(nameof(ItSystemUsageOverviewReadModel.ActiveArchivePeriodEndDate)));
        }

        private static void BindDataProcessingRegistrationModule(ODataConventionModelBuilder builder)
        {
            //Read model to provide slim search options
            BindEntitySet<DataProcessingRegistrationReadModel, DataProcessingRegistrationReadModelsController>(builder);

            //Generic global options
            BindEntitySet<DataProcessingRegistrationRole, DataProcessingRegistrationRolesController>(builder);
            var basisForTransferConfig = BindEntitySet<DataProcessingBasisForTransferOption, DataProcessingBasisForTransferOptionsController>(builder);
            basisForTransferConfig.EntityType.Ignore(x => x.SubDataProcessors);
            BindEntitySet<DataProcessingOversightOption, DataProcessingOversightOptionsController>(builder);
            BindEntitySet<DataProcessingDataResponsibleOption, DataProcessingDataResponsibleOptionsController>(builder);
            var countryConfig = BindEntitySet<DataProcessingCountryOption, DataProcessingCountryOptionsController>(builder);
            countryConfig.EntityType.Ignore(x => x.SubDataProcessors);

            //Generic local options
            BindEntitySet<LocalDataProcessingBasisForTransferOption, LocalDataProcessingBasisForTransferOptionsController>(builder);
            BindEntitySet<LocalDataProcessingOversightOption, LocalDataProcessingOversightOptionsController>(builder);
            BindEntitySet<LocalDataProcessingDataResponsibleOption, LocalDataProcessingDataResponsibleOptionsController>(builder);
            BindEntitySet<LocalDataProcessingCountryOption, LocalDataProcessingCountryOptionsController>(builder);
            BindEntitySet<LocalDataProcessingRegistrationRole, LocalDataProcessingRegistrationRolesController>(builder);

            //Remove parent from dpa to prevent expand
            builder.StructuralTypes.First(t => t.ClrType == typeof(DataProcessingRegistrationReadModel)).RemoveProperty(typeof(DataProcessingRegistrationReadModel).GetProperty(nameof(DataProcessingRegistrationReadModel.SourceEntity)));
            builder.StructuralTypes.First(t => t.ClrType == typeof(DataProcessingRegistrationReadModel)).RemoveProperty(typeof(DataProcessingRegistrationReadModel).GetProperty(nameof(DataProcessingRegistrationReadModel.Organization)));
        }

        private static EntitySetConfiguration<TEntitySet> BindEntitySet<TEntitySet, TController>(ODataConventionModelBuilder builder) where TEntitySet : class, IHasId
        {
            var entitySetConfiguration = BindTypeSet<TEntitySet, TController>(builder);
            entitySetConfiguration.EntityType.HasKey(x => x.Id);
            return entitySetConfiguration;
        }

        private static EntitySetConfiguration<TEntitySet> BindTypeSet<TEntitySet, TController>(ODataConventionModelBuilder builder) where TEntitySet : class
        {
            return builder.EntitySet<TEntitySet>(typeof(TController).Name.Replace(ControllerSuffix, string.Empty));
        }
    }
}
