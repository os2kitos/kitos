using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Core.DomainModel.Organization;
using Core.DomainModel.Reports;
using Microsoft.OData.Edm;
using Presentation.Web.Controllers.API;
using Presentation.Web.Controllers.OData;
using Presentation.Web.Controllers.OData.LocalOptionControllers;
using Core.DomainModel.LocalOptions;
using Presentation.Web.Controllers.OData.OptionControllers;
using Presentation.Web.Infrastructure;
using Core.DomainModel.Advice;
using Core.DomainModel.AdviceSent;
using Presentation.Web.Models;
using System.Linq;

namespace Presentation.Web
{
    using Controllers.OData.AttachedOptions;
    using DocumentFormat.OpenXml.Wordprocessing;
    using Microsoft.OData;
    using Microsoft.OData.UriParser;
    using System;
    using System.Collections.Generic;
    using System.Web.OData.Routing.Conventions;
    using DataType = Core.DomainModel.ItSystem.DataType;
    using HelpText = Core.DomainModel.HelpText;

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
            config.MapHttpAttributeRoutes();
            var apiCfg = config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();
            var routeName = "odata";

            var route = config.MapODataServiceRoute(routeName: routeName, routePrefix: "odata", configureAction: (builder => builder
            .AddService(ServiceLifetime.Singleton, sp => GetModel())
            .AddService(ServiceLifetime.Singleton, sp => new StringAsEnumResolver())
            .AddService<ODataUriResolver>(ServiceLifetime.Singleton, sp => new CaseInsensitiveResolver())
            .AddService(ServiceLifetime.Singleton, sp => new UnqualifiedODataUriResolver())
            .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                        ODataRoutingConventions.CreateDefaultWithAttributeRouting(routeName, config))));
            
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Filters.Add(new ExceptionLogFilterAttribute());
            config.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
        }

        public static IEdmModel GetModel()
        {
            var builder = new ODataConventionModelBuilder();

            // BUG with EnableLowerCamelCase http://stackoverflow.com/questions/39269261/odata-complains-about-missing-id-property-when-enabling-camelcasing
            //builder.EnableLowerCamelCase();

            var accessMod = builder.AddEnumType(typeof(AccessModifier));
            accessMod.Namespace = "Kitos";
            var orgRoles = builder.AddEnumType(typeof(OrganizationRole));
            orgRoles.Namespace = "Kitos";
            var ObjectTypes = builder.AddEnumType(typeof(ObjectType));
            ObjectTypes.Namespace = "Kitos";
            var Schedulings = builder.AddEnumType(typeof(Scheduling));
            Schedulings.Namespace = "Kitos";
            var OptionsTypes = builder.AddEnumType(typeof(OptionType));
            OptionsTypes.Namespace = "Kitos";

            var organizationRightEntitySetName = nameof(OrganizationRightsController).Replace("Controller", string.Empty);
            var organizationRights = builder.EntitySet<OrganizationRight>(organizationRightEntitySetName);
            organizationRights.HasRequiredBinding(o => o.Organization, "Organizations");
            organizationRights.EntityType.HasKey(x => x.Id);
            

            //builder.EntitySet<Advice>("Advices");

            var agreementElementTypes = builder.EntitySet<AgreementElementType>(nameof(AgreementElementTypesController).Replace("Controller", string.Empty));
            agreementElementTypes.EntityType.HasKey(x => x.Id);


            var ItContractAgreementElementTypes = builder.EntitySet<ItContractAgreementElementTypes>("ItContractAgreementElementTypes");
            ItContractAgreementElementTypes.EntityType.HasKey(x => x.ItContract_Id).HasKey(x => x.AgreementElementType_Id);


            //builder.EntitySet<BusinessType>("BusinessTypes");
            //builder.EntitySet<Communication>("Communications");
            //builder.EntitySet<Config>("Configs");

            var itContractTemplateTypes = builder.EntitySet<ItContractTemplateType>(nameof(ItContractTemplateTypesController).Replace("Controller", string.Empty));
            itContractTemplateTypes.EntityType.HasKey(x => x.Id);

            var itContractTypes = builder.EntitySet<ItContractType>(nameof(ItContractTypesController).Replace("Controller", string.Empty));
            itContractTypes.EntityType.HasKey(x => x.Id);

            var dataRowUsage = builder.EntitySet<DataRowUsage>("DataRowUsages");
            dataRowUsage.EntityType.HasKey(x => new { x.DataRowId, x.ItSystemUsageId, x.ItSystemId, x.ItInterfaceId });

            //builder.EntitySet<EconomyYear>("EconomyYears");

            var economyStream = builder.EntitySet<EconomyStream>("EconomyStreams");
            economyStream.EntityType.HasKey(x => x.Id);

            var economyFunc = builder.Function("ExternEconomyStreams");
            economyFunc.Parameter<int>("Organization");
            economyFunc.ReturnsCollectionFromEntitySet<EconomyStream>("EconomyStreams");

            var frequencyTypes = builder.EntitySet<FrequencyType>(nameof(FrequencyTypesController).Replace("Controller", string.Empty));
            frequencyTypes.EntityType.HasKey(x => x.Id);

            //builder.EntitySet<Goal>("Goals");
            //builder.EntitySet<GoalStatus>("GoalStatus");
            var goalTypes = builder.EntitySet<GoalType>(nameof(GoalTypesController).Replace("Controller", string.Empty));
            goalTypes.EntityType.HasKey(x => x.Id);
            //builder.EntitySet<Handover>("Handovers");
            //builder.EntitySet<HandoverTrial>("HandoverTrials");

            var handoverTrialTypes = builder.EntitySet<HandoverTrialType>(nameof(HandoverTrialTypesController).Replace("Controller", string.Empty));
            handoverTrialTypes.EntityType.HasKey(x => x.Id);

            //builder.EntitySet<Interface>("Interfaces");
            //builder.EntitySet<ItInterfaceExhibit>("ItInterfaceExhibits");
            //builder.EntitySet<ItInterfaceExhibitUsage>("ItInterfaceExhibtUsages");
            //builder.EntitySet<ItSystemUsageOrgUnitUsage>("ItSystemUsageOrgUnitUsages");

            var itContractRights = builder.EntitySet<ItContractRight>(nameof(ItContractRightsController).Replace("Controller", string.Empty));
            itContractRights.EntityType.HasKey(x => x.Id);

            var itContractRoles = builder.EntitySet<ItContractRole>(nameof(ItContractRolesController).Replace("Controller", string.Empty));
            itContractRoles.EntityType.HasKey(x => x.Id);

            //builder.EntitySet<ItProjectStatus>("ItProjectStatuses");

            var itProjectRights = builder.EntitySet<ItProjectRight>(nameof(ItProjectRightsController).Replace("Controller", string.Empty));
            itProjectRights.EntityType.HasKey(x => x.Id);

            var itProjectRoles = builder.EntitySet<ItProjectRole>(nameof(ItProjectRolesController).Replace("Controller", string.Empty));
            itProjectRoles.EntityType.HasKey(x => x.Id);

            var AttachedOptions = builder.EntitySet<AttachedOption>(nameof(AttachedOptionsController).Replace("Controller", string.Empty));
            AttachedOptions.EntityType.HasKey(x => x.Id);

            var itProjectOrgUnitUsage = builder.EntitySet<ItProjectOrgUnitUsage>("ItProjectOrgUnitUsages"); // no controller yet
            itProjectOrgUnitUsage.EntityType.HasKey(x => new { x.ItProjectId, x.OrganizationUnitId });

            var itProject = builder.EntitySet<ItProject>(nameof(ItProjectsController).Replace("Controller", string.Empty));
            itProject.HasRequiredBinding(o => o.Organization, "Organizations");
            itProject.EntityType.HasKey(x => x.Id);

            var interfaceUsage = builder.EntitySet<ItInterfaceUsage>("ItInterfaceUsages"); // no controller yet
            interfaceUsage.EntityType.HasKey(x => new { x.ItSystemUsageId, x.ItSystemId, x.ItInterfaceId });

            var dataOption = builder.EntitySet<DataType>(nameof(DataTypesController).Replace("Controller", string.Empty));
            dataOption.EntityType.HasKey(x => x.Id);

            var dataRow = builder.EntitySet<DataRow>("DataRows"); // no controller yet
            dataRow.EntityType.HasKey(x => x.Id);

            var archiveLocation = builder.EntitySet<ArchiveLocation>(nameof(ArchiveLocationsController).Replace("Controller", string.Empty));
            archiveLocation.EntityType.HasKey(x => x.Id);

            var archiveTestLocation = builder.EntitySet<ArchiveTestLocation>(nameof(ArchiveTestLocationsController).Replace("Controller", string.Empty));
            archiveTestLocation.EntityType.HasKey(x => x.Id);

            var archiveOption = builder.EntitySet<ArchiveType>(nameof(ArchiveTypesController).Replace("Controller", string.Empty));
            archiveOption.EntityType.HasKey(x => x.Id);

            var itSystemCategories = builder.EntitySet<ItSystemCategories>(nameof(ItSystemCategoriesController).Replace("Controller", string.Empty));
            itSystemCategories.EntityType.HasKey(x => x.Id);

            var itSystems = builder.EntitySet<ItSystem>(nameof(ItSystemsController).Replace("Controller", string.Empty));
            itSystems.HasRequiredBinding(o => o.Organization, "Organizations");
            itSystems.HasRequiredBinding(o => o.BelongsTo, "Organizations");
            itSystems.HasManyBinding(i => i.Children, "ItSystems");
            itSystems.HasRequiredBinding(i => i.Parent, "ItSystems");
            itSystems.EntityType.HasKey(x => x.Id);

            var itSystemType = builder.EntitySet<ItSystemType>(nameof(ItSystemTypesController).Replace("Controller", string.Empty));
            itSystemType.HasManyBinding(i => i.References, "ItSystems");
            itSystemType.EntityType.HasKey(x => x.Id);

            var businessTypes = builder.EntitySet<BusinessType>(nameof(BusinessTypesController).Replace("Controller", string.Empty));
            businessTypes.EntityType.HasKey(x => x.Id);
            businessTypes.HasManyBinding(b => b.References, "ItSystems");

            var taskRefs = builder.EntitySet<TaskRef>("TaskRefs"); // no controller yet
            taskRefs.HasManyBinding(t => t.ItSystems, "ItSystems");
            taskRefs.EntityType.HasKey(x => x.Id);
            
            var ReportsMunicipalitiesEntitySetName = nameof(ReportsMunicipalitiesController).Replace("Controller", string.Empty);
            var ReportsMunicipalities = builder.EntitySet<Organization>(ReportsMunicipalitiesEntitySetName);
            ReportsMunicipalities.HasManyBinding(o => o.ItSystems, "ItSystems");
            ReportsMunicipalities.HasManyBinding(o => o.BelongingSystems, "ItSystems");

            var ReportsItSystemsEntitySetName = nameof(ReportsItSystemsController).Replace("Controller", string.Empty);
            var ReportsItSystems = builder.EntitySet<ItSystem>(ReportsItSystemsEntitySetName);
            ReportsItSystems.HasRequiredBinding(o => o.Organization, "Organizations");
            ReportsItSystems.HasRequiredBinding(o => o.BelongsTo, "Organizations");
            ReportsItSystems.HasManyBinding(i => i.Children, "ItSystems");
            ReportsItSystems.HasRequiredBinding(i => i.Parent, "ItSystems");

            //singleton instead of entity type because of navigation conflict with 'ItSystemRoles'
            var ReportsItSystemRolesEntitySetName = nameof(ReportsItSystemRolesController).Replace("Controller", string.Empty);
            var ReportsItSystemRoles = builder.EntitySet<ItSystemRole>(ReportsItSystemRolesEntitySetName);

   

            //singleton instead of entity type because of navigation conflict with 'ItSystemRights'
            var ReportsITSystemContactsEntitySetName = nameof(ReportsITSystemContactsController).Replace("Controller", string.Empty);
            var ReportsITSystemContacts = builder.EntitySet<ReportItSystemRightOutputDTO>(ReportsITSystemContactsEntitySetName);
            ReportsITSystemContacts.EntityType.HasKey(x => x.roleId);


            var orgNameSpace = "Organizations";

            var organizationEntitySetName = nameof(OrganizationsController).Replace("Controller", string.Empty);
            var organizations = builder.EntitySet<Organization>(organizationEntitySetName);
            organizations.EntityType.HasKey(x => x.Id);
            organizations.EntityType.HasMany(x => x.OrgUnits).IsNavigable().Name = "OrganizationUnits";
            organizations.EntityType.Property(p => p.Uuid).IsOptional();

            organizations.HasManyBinding(o => o.ItSystems, "ItSystems");
            organizations.HasManyBinding(o => o.BelongingSystems, "ItSystems");
            
            var removeUserAction = organizations.EntityType.Collection.Action("RemoveUser");
            removeUserAction.Parameter<int>("orgKey").OptionalParameter = false;
            removeUserAction.Parameter<int>("userId").OptionalParameter = false;
            removeUserAction.Namespace = orgNameSpace;
            
            var getAdviceByOrgFunction = organizations.EntityType.Collection.Function("GetByOrganization").ReturnsCollectionFromEntitySet<Advice>("Advice");
            getAdviceByOrgFunction.Parameter<int>("userId").OptionalParameter = false;
            getAdviceByOrgFunction.ReturnsCollectionFromEntitySet<Advice>(nameof(Controllers.OData.AdviceController).Replace("Controller", string.Empty));
            getAdviceByOrgFunction.Namespace = orgNameSpace;

            var orgUnits = builder.EntitySet<OrganizationUnit>(nameof(OrganizationUnitsController).Replace("Controller", string.Empty));
            orgUnits.HasRequiredBinding(o => o.Organization, "Organizations");
            orgUnits.EntityType.HasKey(x => x.Id);
            orgUnits.EntityType.HasMany(x => x.ResponsibleForItContracts).Name = "ItContracts";
            orgUnits.EntityType.HasMany(x => x.UsingItProjects).Name = "ItProjects";
            //Add isActive to result form odata
            builder.StructuralTypes.First(t => t.ClrType == typeof(ItContract)).AddProperty(typeof(ItContract).GetProperty("IsActive"));

            var userNameSpace = "Users";
            var userEntitySetName = nameof(UsersController).Replace("Controller", string.Empty);
            var users = builder.EntitySet<User>(userEntitySetName);
            users.HasRequiredBinding(u => u.DefaultOrganization, "Organizations");
            users.EntityType.HasKey(x => x.Id);
            users.EntityType.Ignore(x => x.Password);
            users.EntityType.Ignore(x => x.Salt);
            users.EntityType.Property(x => x.Name).IsRequired();
            users.EntityType.Property(x => x.Email).IsRequired();
            
            var orgGetUsersFunction = organizations.EntityType.Function("GetUsers").ReturnsCollectionFromEntitySet<User>(userEntitySetName);
            orgGetUsersFunction.Namespace = orgNameSpace;

            var userCreateAction = users.EntityType.Collection.Action("Create").ReturnsFromEntitySet<User>(userEntitySetName);
            userCreateAction.Namespace = userNameSpace;
            userCreateAction.Parameter<User>("user").OptionalParameter = false;
            userCreateAction.Parameter<int>("organizationId").OptionalParameter = false;
            userCreateAction.Parameter<bool>("sendMailOnCreation").OptionalParameter = true;

            var userCheckEmailFunction = users.EntityType.Collection.Function("IsEmailAvailable").Returns<bool>();
            userCheckEmailFunction.Parameter<string>("email").OptionalParameter = false;
            userCheckEmailFunction.Namespace = userNameSpace;

            var userGetByMailFunction = builder.Function("GetUserByEmail").ReturnsFromEntitySet<User>(userEntitySetName);
            userGetByMailFunction.Parameter<string>("email").OptionalParameter = false;

            var usages = builder.EntitySet<ItSystemUsage>(nameof(ItSystemUsagesController).Replace("Controller", string.Empty));
            usages.HasRequiredBinding(u => u.Organization, "Organizations");
            usages.HasRequiredBinding(u => u.ItSystem, "ItSystems");
            usages.EntityType.HasKey(x => x.Id);

            var itSystemRights = builder.EntitySet<ItSystemRight>(nameof(ItSystemRightsController).Replace("Controller", string.Empty));
            itSystemRights.HasRequiredBinding(u => u.Role, "ItSystemRoles");
            itSystemRights.EntityType.HasKey(x => x.Id);

            var roles = builder.EntitySet<ItSystemRole>(nameof(ItSystemRolesController).Replace("Controller", string.Empty));
            roles.EntityType.HasKey(x => x.Id);

            var systemOrgUnitUsages = builder.EntitySet<ItSystemUsageOrgUnitUsage>("ItSystemUsageOrgUnitUsages"); // no controller yet
            systemOrgUnitUsages.EntityType.HasKey(x => x.ItSystemUsageId).HasKey(x => x.OrganizationUnitId);

            var contractItSystemUsages = builder.EntitySet<ItContractItSystemUsage>("ItContractItSystemUsages"); // no controller yet
            contractItSystemUsages.EntityType.HasKey(x => x.ItContractId).HasKey(x => x.ItSystemUsageId);
            builder.StructuralTypes.First(t => t.ClrType == typeof(ItSystemUsage)).AddProperty(typeof(ItSystemUsage).GetProperty("IsActive"));

            var contracts = builder.EntitySet<ItContract>(nameof(ItContractsController).Replace("Controller", string.Empty));
            contracts.HasRequiredBinding(o => o.Organization, "Organizations");
            contracts.HasRequiredBinding(o => o.Supplier, "Organizations");
            contracts.EntityType.HasKey(x => x.Id);
            contracts.EntityType.HasMany(x => x.ExternEconomyStreams).IsNotExpandable(); // do not remove
            contracts.EntityType.HasMany(x => x.InternEconomyStreams).IsNotExpandable(); // do not remove

            // TODO this field is causing issues.
            // This query fails: /odata/Organizations(1)/ItSystemUsages?$expand=MainContract($expand=ItContract)

            var interfaceTypes = builder.EntitySet<InterfaceType>(nameof(InterfaceTypesController).Replace("Controller", string.Empty));
            interfaceTypes.EntityType.HasKey(x => x.Id);

            var itInterfaces = builder.EntitySet<ItInterface>(nameof(ItInterfacesController).Replace("Controller", string.Empty));
            itInterfaces.HasRequiredBinding(o => o.Organization, "Organizations");
            itInterfaces.HasRequiredBinding(o => o.BelongsTo, "Organizations");
            itInterfaces.EntityType.HasKey(x => x.Id);

            var itInterfaceTypes = builder.EntitySet<ItInterfaceType>(nameof(ItInterfaceTypesController).Replace("Controller", string.Empty));
            itInterfaceTypes.EntityType.HasKey(x => x.Id);

            var itInterfaceExihibits = builder.EntitySet<ItInterfaceExhibit>("ItInterfaceExhibits"); // no controller yet
            itInterfaceExihibits.HasRequiredBinding(o => o.ItSystem, "ItSystems");
            itInterfaceExihibits.EntityType.HasKey(x => x.Id);

            var itInterfaceExhibitUsage = builder.EntitySet<ItInterfaceExhibitUsage>("ItInterfaceExhibitUsages"); // no controller yet
            itInterfaceExhibitUsage.EntityType.HasKey(x => x.ItContractId)
                .HasKey(x => x.ItInterfaceExhibitId)
                .HasKey(x => x.ItSystemUsageId);

            // Udkommenteret ifm. OS2KITOS-663
            //var itInterfaceUse = builder.EntitySet<ItInterfaceUse>(nameof(ItInterfaceUsesEntityController).Replace("Controller", string.Empty));
            //itInterfaceUse.EntityType
            //    .HasKey(x => x.ItSystemId)
            //    .HasKey(x => x.ItInterfaceId);

            var tsas = builder.EntitySet<TsaType>(nameof(TsaTypesController).Replace("Controller", string.Empty));
            tsas.EntityType.HasKey(x => x.Id);

            var methodTypes = builder.EntitySet<MethodType>(nameof(MethodTypesController).Replace("Controller", string.Empty));
            methodTypes.EntityType.HasKey(x => x.Id);

            var sensitiveDataOption = builder.EntitySet<SensitiveDataType>(nameof(SensitiveDataTypesController).Replace("Controller", string.Empty));
            sensitiveDataOption.EntityType.HasKey(x => x.Id);

           /* var RegularPersonalDataTypes = builder.EntitySet<RegularPersonalDataType>(nameof(RegularPersonalDataTypesController).Replace("Controller", string.Empty));
            RegularPersonalDataTypes.EntityType.HasKey(x => x.Id);
            RegularPersonalDataTypes.HasManyBinding(b => b.References, "ItSystems");
            */
            var RegisterTypes = builder.EntitySet<RegisterType>(nameof(RegisterTypesController).Replace("Controller", string.Empty));
            RegisterTypes.EntityType.HasKey(x => x.Id);

            var SensitivePersonalDataTypes = builder.EntitySet<SensitivePersonalDataType>(nameof(SensistivePersonalDataTypesController).Replace("Controller", string.Empty));
            SensitivePersonalDataTypes.EntityType.HasKey(x => x.Id);
            SensitivePersonalDataTypes.HasManyBinding(b => b.References, "ItSystems");

            var optionExtendTypes = builder.EntitySet<OptionExtendType>(nameof(OptionExtendTypesController).Replace("Controller", string.Empty));
            optionExtendTypes.EntityType.HasKey(x => x.Id);

            var organizationUnitRights = builder.EntitySet<OrganizationUnitRight>(nameof(OrganizationUnitRightsController).Replace("Controller", string.Empty));
            organizationUnitRights.EntityType.HasKey(x => x.Id);

            var organiationUnitRoles = builder.EntitySet<OrganizationUnitRole>(nameof(OrganizationUnitRolesController).Replace("Controller", string.Empty));
            organiationUnitRoles.EntityType.HasKey(x => x.Id);

            //builder.EntitySet<PasswordResetRequest>("PasswordResetRequests");

            var paymentFreqencyTypes = builder.EntitySet<PaymentFreqencyType>(nameof(PaymentFrequencyTypesController).Replace("Controller", string.Empty));
            paymentFreqencyTypes.EntityType.HasKey(x => x.Id);

            //builder.EntitySet<PaymentMilestone>("PaymentMilestones");
            //builder.EntitySet<PaymentModelType>("PaymentModelTypes");

            var paymentModelTypes = builder.EntitySet<PaymentModelType>(nameof(PaymentModelTypesController).Replace("Controller", string.Empty));
            paymentModelTypes.EntityType.HasKey(x => x.Id);

            var priceRegulationTypes = builder.EntitySet<PriceRegulationType>(nameof(PriceRegulationTypesController).Replace("Controller", string.Empty));
            priceRegulationTypes.EntityType.HasKey(x => x.Id);

            // These two lines causes an 404 error when requesting odata/ProcurementStrategyTypes at https://localhost:44300/#/global-config/contract
            // Requesting api/ProcurementStrategy works but not odata/ProcurementStrategyTypes
            //var procurementStrategy = builder.EntitySet<ProcurementStrategyType>(nameof(ProcurementStrategyController).Replace("Controller", string.Empty));
            //procurementStrategy.EntityType.HasKey(x => x.Id);

            // There two lines fixes the 404 error at https://localhost:44300/#/global-config/contract
            // Requesting api/ProcurementStrategy and odata/ProcurementStrategyTypes both work
            var procurementStrategyTypes = builder.EntitySet<ProcurementStrategyType>(nameof(ProcurementStrategyTypesController).Replace("Controller", string.Empty));
            procurementStrategyTypes.EntityType.HasKey(x => x.Id);

            var itProjectTypes = builder.EntitySet<ItProjectType>(nameof(ItProjectTypesController).Replace("Controller", string.Empty));
            itProjectTypes.EntityType.HasKey(x => x.Id);

            var purchaseFormType = builder.EntitySet<PurchaseFormType>(nameof(PurchaseFormTypesController).Replace("Controller", string.Empty));
            purchaseFormType.EntityType.HasKey(x => x.Id);

            //Local options

            var LocalAgreementElementType = builder.EntitySet<LocalAgreementElementType>(nameof(LocalAgreementElementTypesController).Replace("Controller", string.Empty));
            LocalAgreementElementType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalAgreementElementType.EntityType.HasKey(x => x.Id);


            var LocalArchiveType = builder.EntitySet<LocalArchiveType>(nameof(LocalArchiveTypesController).Replace("Controller", string.Empty));
            LocalArchiveType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalArchiveType.EntityType.HasKey(x => x.Id);

            var LocalArchiveLocation = builder.EntitySet<LocalArchiveLocation>(nameof(LocalArchiveLocationsController).Replace("Controller", string.Empty));
            LocalArchiveLocation.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalArchiveLocation.EntityType.HasKey(x => x.Id);

            var LocalArchiveTestLocation = builder.EntitySet<LocalArchiveTestLocation>(nameof(LocalArchiveTestLocationsController).Replace("Controller", string.Empty));
            LocalArchiveTestLocation.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalArchiveTestLocation.EntityType.HasKey(x => x.Id);

            var LocalItSystemCategories = builder.EntitySet<LocalItSystemCategories>(nameof(LocalItSystemCategoriesController).Replace("Controller", string.Empty));
            LocalItSystemCategories.HasRequiredBinding(x => x.Organization, "Organizations");
            LocalItSystemCategories.EntityType.HasKey(x => x.Id);

            var LocalBusinessType = builder.EntitySet<LocalBusinessType>(nameof(LocalBusinessTypesController).Replace("Controller", string.Empty));
            LocalBusinessType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalBusinessType.EntityType.HasKey(x => x.Id);

            var LocalDataType = builder.EntitySet<LocalDataType>(nameof(LocalDataTypesController).Replace("Controller", string.Empty));
            LocalDataType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalDataType.EntityType.HasKey(x => x.Id);

            var LocalFrequencyType = builder.EntitySet<LocalFrequencyType>(nameof(LocalFrequencyTypesController).Replace("Controller", string.Empty));
            LocalFrequencyType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalFrequencyType.EntityType.HasKey(x => x.Id);

            var LocalGoalType = builder.EntitySet<LocalGoalType>(nameof(LocalGoalTypesController).Replace("Controller", string.Empty));
            LocalGoalType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalGoalType.EntityType.HasKey(x => x.Id);

            var LocalHandoverTrialType = builder.EntitySet<LocalHandoverTrialType>(nameof(LocalHandoverTrialTypesController).Replace("Controller", string.Empty));
            LocalHandoverTrialType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalHandoverTrialType.EntityType.HasKey(x => x.Id);

            var LocalInterfaceType = builder.EntitySet<LocalInterfaceType>(nameof(LocalInterfaceTypesController).Replace("Controller", string.Empty));
            LocalInterfaceType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalInterfaceType.EntityType.HasKey(x => x.Id);

            var LocalItContractRole = builder.EntitySet<LocalItContractRole>(nameof(LocalItContractRolesController).Replace("Controller", string.Empty));
            LocalItContractRole.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalItContractRole.EntityType.HasKey(x => x.Id);

            var LocalItContractTemplateType = builder.EntitySet<LocalItContractTemplateType>(nameof(LocalItContractTemplateTypesController).Replace("Controller", string.Empty));
            LocalItContractTemplateType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalItContractTemplateType.EntityType.HasKey(x => x.Id);

            var LocalItContractType = builder.EntitySet<LocalItContractType>(nameof(LocalItContractTypesController).Replace("Controller", string.Empty));
            LocalItContractType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalItContractType.EntityType.HasKey(x => x.Id);

            var LocalItInterfaceType = builder.EntitySet<LocalItInterfaceType>(nameof(LocalItInterfaceTypesController).Replace("Controller", string.Empty));
            LocalItInterfaceType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalItInterfaceType.EntityType.HasKey(x => x.Id);

            var LocalItProjectRole = builder.EntitySet<LocalItProjectRole>(nameof(LocalItProjectRolesController).Replace("Controller", string.Empty));
            LocalItProjectRole.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalItProjectRole.EntityType.HasKey(x => x.Id);

            var LocalItProjectType = builder.EntitySet<LocalItProjectType>(nameof(LocalItProjectTypesController).Replace("Controller", string.Empty));
            LocalItProjectType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalItProjectType.EntityType.HasKey(x => x.Id);

            var LocalItSystemRole = builder.EntitySet<LocalItSystemRole>(nameof(LocalItSystemRolesController).Replace("Controller", string.Empty));
            LocalItSystemRole.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalItSystemRole.EntityType.HasKey(x => x.Id);

            var LocalItSystemType = builder.EntitySet<LocalItSystemType>(nameof(LocalItSystemTypesController).Replace("Controller", string.Empty));
            LocalItSystemType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalItSystemType.EntityType.HasKey(x => x.Id);

            var LocalMethodType = builder.EntitySet<LocalMethodType>(nameof(LocalMethodTypesController).Replace("Controller", string.Empty));
            LocalMethodType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalMethodType.EntityType.HasKey(x => x.Id);

            var LocalOptionExtendType = builder.EntitySet<LocalOptionExtendType>(nameof(LocalOptionExtendTypesController).Replace("Controller", string.Empty));
            LocalOptionExtendType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalOptionExtendType.EntityType.HasKey(x => x.Id);

            var LocalPaymentFreqencyType = builder.EntitySet<LocalPaymentFreqencyType>(nameof(LocalPaymentFrequencyTypesController).Replace("Controller", string.Empty));
            LocalPaymentFreqencyType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalPaymentFreqencyType.EntityType.HasKey(x => x.Id);

            var LocalPaymentModelType = builder.EntitySet<LocalPaymentModelType>(nameof(LocalPaymentModelTypesController).Replace("Controller", string.Empty));
            LocalPaymentModelType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalPaymentModelType.EntityType.HasKey(x => x.Id);

            var LocalPriceRegulationType = builder.EntitySet<LocalPriceRegulationType>(nameof(LocalPriceRegulationTypesController).Replace("Controller", string.Empty));
            LocalPriceRegulationType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalPriceRegulationType.EntityType.HasKey(x => x.Id);

            var LocalProcurementStrategyType = builder.EntitySet<LocalProcurementStrategyType>(nameof(LocalProcurementStrategyTypesController).Replace("Controller", string.Empty));
            LocalProcurementStrategyType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalProcurementStrategyType.EntityType.HasKey(x => x.Id);

            var LocalPurchaseFormType = builder.EntitySet<LocalPurchaseFormType>(nameof(LocalPurchaseFormTypesController).Replace("Controller", string.Empty));
            LocalPurchaseFormType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalPurchaseFormType.EntityType.HasKey(x => x.Id);

            var LocalReportCategoryType = builder.EntitySet<LocalReportCategoryType>(nameof(LocalReportCategoryTypesController).Replace("Controller", string.Empty));
            LocalReportCategoryType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalReportCategoryType.EntityType.HasKey(x => x.Id);

             var RemoveOption = builder.Function("RemoveOption");
             RemoveOption.Parameter<int>("id");
             RemoveOption.Parameter<int>("objectId");
             RemoveOption.Parameter<int>("type");
             RemoveOption.Parameter<int>("entityType");
             RemoveOption.Returns<IHttpActionResult>();
            
            var GetSensitivePersonalDataByUsageId = builder.Function("GetSensitivePersonalDataByUsageId");
            GetSensitivePersonalDataByUsageId.Parameter<int>("id");
            GetSensitivePersonalDataByUsageId.ReturnsCollectionFromEntitySet<SensitivePersonalDataType>("SensistivePersonalDataTypes");
             builder.StructuralTypes.First(t => t.ClrType == typeof(SensitivePersonalDataType)).AddProperty(typeof(SensitivePersonalDataType).GetProperty("Checked"));
            GetSensitivePersonalDataByUsageId.Namespace = "gdpr";

            var GetSensitivePersonalDataBySystemId = builder.Function("GetSensitivePersonalDataBySystemId");
            GetSensitivePersonalDataBySystemId.Parameter<int>("id");
            GetSensitivePersonalDataBySystemId.ReturnsCollectionFromEntitySet<SensitivePersonalDataType>("SensistivePersonalDataTypes");
            builder.StructuralTypes.First(t => t.ClrType == typeof(SensitivePersonalDataType)).AddProperty(typeof(SensitivePersonalDataType).GetProperty("Checked"));
            GetSensitivePersonalDataBySystemId.Namespace = "gdpr";

            var GetRegularPersonalDataBySystemId = builder.Function("GetRegularPersonalDataBySystemId");
            GetRegularPersonalDataBySystemId.ReturnsCollectionFromEntitySet<RegularPersonalDataType>("RegularPersonalDataTypes")
                .Parameter<int>("id");
             builder.StructuralTypes.First(t => t.ClrType == typeof(RegularPersonalDataType)).AddProperty(typeof(RegularPersonalDataType).GetProperty("Checked"));

            var GetRegularPersonalDataByUsageId = builder.Function("GetRegularPersonalDataByUsageId");
            GetRegularPersonalDataByUsageId.ReturnsCollectionFromEntitySet<RegularPersonalDataType>("RegularPersonalDataTypes")
                .Parameter<int>("id");
            builder.StructuralTypes.First(t => t.ClrType == typeof(RegularPersonalDataType)).AddProperty(typeof(RegularPersonalDataType).GetProperty("Checked"));

            var GetRegisterTypeByObjectID = builder.Function("GetRegisterTypesByObjectID");
             GetRegisterTypeByObjectID.Parameter<int>("id");
             GetRegisterTypeByObjectID.ReturnsCollectionFromEntitySet<RegisterType>("RegisterTypes");
             builder.StructuralTypes.First(t => t.ClrType == typeof(RegisterType)).AddProperty(typeof(RegisterType).GetProperty("Checked"));

             var LocalSensitiveDataType = builder.EntitySet<LocalSensitiveDataType>(nameof(LocalSensitiveDataTypesController).Replace("Controller", string.Empty));
             LocalSensitiveDataType.HasRequiredBinding(u => u.Organization, "Organizations");
             LocalSensitiveDataType.EntityType.HasKey(x => x.Id);

             var LocalSensistivePersonalDataTypes = builder.EntitySet<LocalSensitivePersonalDataType>(nameof(LocalSensistivePersonalDataTypesController).Replace("Controller", string.Empty));
             LocalSensistivePersonalDataTypes.HasRequiredBinding(u => u.Organization, "Organizations");
             LocalSensistivePersonalDataTypes.EntityType.HasKey(x => x.Id);

             var LocalRegularPersonalDataTypes = builder.EntitySet<LocalRegularPersonalDataType>(nameof(LocalRegularPersonalDataTypesController).Replace("Controller", string.Empty));
             LocalRegularPersonalDataTypes.HasRequiredBinding(u => u.Organization, "Organizations");
             LocalRegularPersonalDataTypes.EntityType.HasKey(x => x.Id);
           
            var LocalRegisterTypes = builder.EntitySet<LocalRegisterType>(nameof(LocalRegisterTypesController).Replace("Controller", string.Empty));
            LocalRegisterTypes.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalRegisterTypes.EntityType.HasKey(x => x.Id);
            
            var LocalTerminationDeadlineType = builder.EntitySet<LocalTerminationDeadlineType>(nameof(LocalTerminationDeadlineTypesController).Replace("Controller", string.Empty));
            LocalTerminationDeadlineType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalTerminationDeadlineType.EntityType.HasKey(x => x.Id);

            var LocalTsaType = builder.EntitySet<LocalTsaType>(nameof(LocalTsaTypesController).Replace("Controller", string.Empty));
            LocalTsaType.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalTsaType.EntityType.HasKey(x => x.Id);

            var LocalOrganizationUnitRole = builder.EntitySet<LocalOrganizationUnitRole>(nameof(LocalOrganizationUnitRolesController).Replace("Controller", string.Empty));
            LocalOrganizationUnitRole.HasRequiredBinding(u => u.Organization, "Organizations");
            LocalOrganizationUnitRole.EntityType.HasKey(x => x.Id);

            //builder.EntitySet<Risk>("Risks");
            //builder.EntitySet<Stakeholder>("Stakeholders");

            var terminationDeadlineType = builder.EntitySet<TerminationDeadlineType>(nameof(TerminationDeadlineTypesController).Replace("Controller", string.Empty));
            terminationDeadlineType.EntityType.HasKey(x => x.Id);

            var config = builder.EntitySet<Config>(nameof(ConfigsController).Replace("Controller", string.Empty));
            config.HasRequiredBinding(u => u.Organization, "Organizations");
            config.EntityType.HasKey(x => x.Id);


            var Advice = builder.EntitySet<Advice>(nameof(Controllers.OData.AdviceController).Replace("Controller", string.Empty));
            Advice.EntityType.HasKey(x => x.Id);

            var adviceSent = builder.EntitySet<AdviceSent>(nameof(AdviceSentController).Replace("Controller", string.Empty));
            adviceSent.EntityType.HasKey(x => x.Id);
            // var GetByObjectId = users.EntityType.Collection.Function("GetByObjectId").Returns<Icolle>();

            var GetAdvicesByObjectID = builder.Function("GetAdvicesByObjectID");
            GetAdvicesByObjectID.Parameter<int>("id");
            GetAdvicesByObjectID.Parameter<int>("type");
            GetAdvicesByObjectID.ReturnsCollectionFromEntitySet<Advice>("Advice");


            var globalConfig = builder.EntitySet<GlobalConfig>(nameof(GlobalConfigsController).Replace("Controller", string.Empty));
            globalConfig.EntityType.HasKey(x => x.Id);

            var accessType = builder.EntitySet<AccessType>(nameof(AccessTypesController).Replace("Controller", string.Empty));
            accessType.HasRequiredBinding(a => a.ItSystem, "ItSystems");
            accessType.EntityType.HasKey(x => x.Id);

            var archivePeriod = builder.EntitySet<ArchivePeriod>(nameof(ArchivePeriodsController).Replace("Controller", string.Empty));
            archivePeriod.EntityType.HasKey(x => x.Id);

            //builder.EntitySet<TaskRef>("TaskRefs");
            //builder.EntitySet<TaskUsage>("TaskUsages");
            //builder.EntitySet<Text>("Texts");
            //builder.EntitySet<User>("Users");
            //builder.EntitySet<Wish>("Wishes");

            var reports = builder.EntitySet<Report>("Reports");
            reports.HasRequiredBinding(u => u.Organization, "Organizations");
            reports.EntityType.HasKey(x => x.Id);

            var references = builder.EntitySet<ExternalReference>("ExternalReferences");
            references.EntityType.HasKey(x => x.Id);
            references.HasRequiredBinding(a => a.ItSystem, "ItSystems");

            var reportCategoryTypes = builder.EntitySet<ReportCategoryType>(nameof(ReportCategoryTypesController).Replace("Controller", string.Empty));
            reportCategoryTypes.EntityType.HasKey(x => x.Id);

            var helpTexts = builder.EntitySet<HelpText>(nameof(HelpTextsController).Replace("Controller", string.Empty));
            helpTexts.EntityType.HasKey(x => x.Id);

            var itProjectStatusUpdates = builder.EntitySet<ItProjectStatusUpdate>(nameof(ItProjectStatusUpdatesController).Replace("Controller", string.Empty));
            itProjectStatusUpdates.EntityType.HasKey(x => x.Id);
            itProjectStatusUpdates.HasRequiredBinding(o => o.Organization, "Organizations");


            return builder.GetEdmModel();
        }

        //For making urls case insensitive
        internal class CaseInsensitiveResolver : ODataUriResolver
        {
            private bool _enableCaseInsensitive;

            public override bool EnableCaseInsensitive
            {
                get { return true; }
                set { _enableCaseInsensitive = value; }
            }
        }

    }
}
