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
using Presentation.Web.Controllers.OData;

namespace Presentation.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
            var apiCfg = config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();

            //OData
            config.MapODataServiceRoute(
                routeName: "odata",
                routePrefix: "odata",
                model: GetModel());

            config.EnableEnumPrefixFree(true);
            config.EnableCaseInsensitive(true);
            config.EnableUnqualifiedNameCall(true);
            config.Formatters.Remove(config.Formatters.XmlFormatter);
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

            var organizationRightEntitySetName = nameof(OrganizationRightsController).Replace("Controller", string.Empty);
            var organizationRights = builder.EntitySet<OrganizationRight>(organizationRightEntitySetName);
            organizationRights.EntityType.HasKey(x => x.Id);

            //builder.EntitySet<Advice>("Advices");
            //builder.EntitySet<AgreementElementType>("AgreementElementTypes");
            //builder.EntitySet<BusinessType>("BusinessTypes");
            //builder.EntitySet<Communication>("Communications");
            //builder.EntitySet<Config>("Configs");
            //builder.EntitySet<ItContractTemplateTypes>("ItContractTemplateTypes");
            //builder.EntitySet<IContractType>("ItContractTypes");

            var dataRowUsage = builder.EntitySet<DataRowUsage>("DataRowUsages");
            dataRowUsage.EntityType.HasKey(x => new { x.DataRowId, x.ItSystemUsageId, x.ItSystemId, x.ItInterfaceId });

            //builder.EntitySet<EconomyYear>("EconomyYears");

            var economyStream = builder.EntitySet<EconomyStream>("EconomyStreams");
            economyStream.EntityType.HasKey(x => x.Id);

            var economyFunc = builder.Function("ExternEconomyStreams");
            economyFunc.Parameter<int>("Organization");
            economyFunc.ReturnsCollectionFromEntitySet<EconomyStream>("EconomyStreams");

            //builder.EntitySet<FrequencyType>("FrequencyTypes");
            //builder.EntitySet<Goal>("Goals");
            //builder.EntitySet<GoalStatus>("GoalStatus");
            //builder.EntitySet<GoalType>("GoalTypes");
            //builder.EntitySet<Handover>("Handovers");
            //builder.EntitySet<HandoverTrial>("HandoverTrials");
            //builder.EntitySet<HandoverTrialType>("HandoverTrialTypes");
            //builder.EntitySet<Interface>("Interfaces");
            //builder.EntitySet<ItInterfaceExhibit>("ItInterfaceExhibits");
            //builder.EntitySet<ItInterfaceExhibitUsage>("ItInterfaceExhibtUsages");
            //builder.EntitySet<InterfaceType>("InterfaceTypes");
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

            var itProjectOrgUnitUsage = builder.EntitySet<ItProjectOrgUnitUsage>("ItProjectOrgUnitUsages"); // no controller yet
            itProjectOrgUnitUsage.EntityType.HasKey(x => new {x.ItProjectId, x.OrganizationUnitId});

            var itProject = builder.EntitySet<ItProject>(nameof(ItProjectsController).Replace("Controller", string.Empty));
            itProject.EntityType.HasKey(x => x.Id);

            var interfaceUsage = builder.EntitySet<ItInterfaceUsage>("ItInterfaceUsages"); // no controller yet
            interfaceUsage.EntityType.HasKey(x => new { x.ItSystemUsageId, x.ItSystemId, x.ItInterfaceId });

            var dataOption = builder.EntitySet<DataType>("DataTypes"); // no controller yet
            dataOption.EntityType.HasKey(x => x.Id);

            var dataRow = builder.EntitySet<DataRow>("DataRows"); // no controller yet
            dataRow.EntityType.HasKey(x => x.Id);

            var archiveOption = builder.EntitySet<ArchiveType>("ArchiveTypes"); // no controller yet
            archiveOption.EntityType.HasKey(x => x.Id);

            var itSystems = builder.EntitySet<ItSystem>(nameof(ItSystemsController).Replace("Controller", string.Empty));
            itSystems.EntityType.HasKey(x => x.Id);

            var itSystemTypeOptions = builder.EntitySet<ItSystemType>("ItSystemTypes"); // no controller yet
            itSystemTypeOptions.EntityType.HasKey(x => x.Id);

            var businessTypes = builder.EntitySet<BusinessType>("BusinessTypes"); // no controller yet
            businessTypes.EntityType.HasKey(x => x.Id);

            var taskRefs = builder.EntitySet<TaskRef>("TaskRefs"); // no controller yet
            taskRefs.EntityType.HasKey(x => x.Id);

            var organizations = builder.EntitySet<Organization>(nameof(OrganizationsController).Replace("Controller", string.Empty));
            organizations.EntityType.HasKey(x => x.Id);
            organizations.EntityType.HasMany(x => x.OrgUnits).IsNavigable().Name = "OrganizationUnits";
            var removeUserAction = organizations.EntityType.Action("RemoveUser");
            removeUserAction.Parameter<int>("userId").OptionalParameter = false;

            var orgUnits = builder.EntitySet<OrganizationUnit>(nameof(OrganizationUnitsController).Replace("Controller", string.Empty));
            orgUnits.EntityType.HasKey(x => x.Id);
            orgUnits.EntityType.HasMany(x => x.ResponsibleForItContracts).Name = "ItContracts";
            orgUnits.EntityType.HasMany(x => x.UsingItProjects).Name = "ItProjects";

            var userEntitySetName = nameof(UsersController).Replace("Controller", string.Empty);
            var users = builder.EntitySet<User>(userEntitySetName);
            users.EntityType.HasKey(x => x.Id);
            users.EntityType.Ignore(x => x.Password);
            users.EntityType.Ignore(x => x.Salt);
            users.EntityType.Property(x => x.Name).IsRequired();
            users.EntityType.Property(x => x.LastName).IsRequired();
            users.EntityType.Property(x => x.Email).IsRequired();
            var userCreateAction = users.EntityType.Collection.Action("Create").ReturnsFromEntitySet<User>(userEntitySetName);
            userCreateAction.Parameter<User>("user").OptionalParameter = false;
            userCreateAction.Parameter<int>("organizationId").OptionalParameter = false;
            userCreateAction.Parameter<bool>("sendMailOnCreation").OptionalParameter = true;
            var userCheckEmailFunction = users.EntityType.Collection.Function("IsEmailAvailable").Returns<bool>();
            userCheckEmailFunction.Parameter<string>("email").OptionalParameter = false;

            var usages = builder.EntitySet<ItSystemUsage>(nameof(ItSystemUsagesController).Replace("Controller", string.Empty));
            usages.EntityType.HasKey(x => x.Id);

            var itSystemRights = builder.EntitySet<ItSystemRight>(nameof(ItSystemRightsController).Replace("Controller", string.Empty));
            itSystemRights.EntityType.HasKey(x => x.Id);

            var roles = builder.EntitySet<ItSystemRole>(nameof(ItSystemRolesController).Replace("Controller", string.Empty));
            roles.EntityType.HasKey(x => x.Id);

            var systemOrgUnitUsages = builder.EntitySet<ItSystemUsageOrgUnitUsage>("ItSystemUsageOrgUnitUsages"); // no controller yet
            systemOrgUnitUsages.EntityType.HasKey(x => x.ItSystemUsageId).HasKey(x => x.OrganizationUnitId);

            var contractItSystemUsages = builder.EntitySet<ItContractItSystemUsage>("ItContractItSystemUsages"); // no controller yet
            contractItSystemUsages.EntityType.HasKey(x => x.ItContractId).HasKey(x => x.ItSystemUsageId);

            var contracts = builder.EntitySet<ItContract>(nameof(ItContractsController).Replace("Controller", string.Empty));
            contracts.EntityType.HasKey(x => x.Id);
            contracts.EntityType.HasMany(x => x.ExternEconomyStreams).IsNotExpandable(); // do not remove
            contracts.EntityType.HasMany(x => x.InternEconomyStreams).IsNotExpandable(); // do not remove

            // TODO this field is causing issues.
            // This query fails: /odata/Organizations(1)/ItSystemUsages?$expand=MainContract($expand=ItContract)
            // if ItContract.Terminated has a value
            contracts.EntityType.Ignore(x => x.IsActive);

            var interfaceTypes = builder.EntitySet<InterfaceType>("InterfaceTypes"); // no controller yet
            interfaceTypes.EntityType.HasKey(x => x.Id);

            var itInterfaces = builder.EntitySet<ItInterface>(nameof(ItInterfacesController).Replace("Controller", string.Empty));
            itInterfaces.EntityType.HasKey(x => x.Id);

            var itInterfaceTypes = builder.EntitySet<ItInterfaceType>("ItInterfaceTypes"); // no controller yet
            itInterfaceTypes.EntityType.HasKey(x => x.Id);

            var itInterfaceExihibits = builder.EntitySet<ItInterfaceExhibit>("ItInterfaceExhibits"); // no controller yet
            itInterfaceExihibits.EntityType.HasKey(x => x.Id);

            var itInterfaceExhibitUsage = builder.EntitySet<ItInterfaceExhibitUsage>("ItInterfaceExhibitUsages"); // no controller yet
            itInterfaceExhibitUsage.EntityType.HasKey(x => x.ItContractId)
                .HasKey(x => x.ItInterfaceExhibitId)
                .HasKey(x => x.ItSystemUsageId);

            var itInterfaceUse = builder.EntitySet<ItInterfaceUse>(nameof(ItInterfaceUsesEntityController).Replace("Controller", string.Empty));
            itInterfaceUse.EntityType
                .HasKey(x => x.ItSystemId)
                .HasKey(x => x.ItInterfaceId);

            var tsas = builder.EntitySet<TsaType>("TsaTypes"); // no controller yet
            tsas.EntityType.HasKey(x => x.Id);

            var methods = builder.EntitySet<MethodType>("MethodTypes"); // no controller yet
            methods.EntityType.HasKey(x => x.Id);

            var sensitiveDataOption = builder.EntitySet<SensitiveDataType>("SensitiveDataTypes"); // no controller yet
            sensitiveDataOption.EntityType.HasKey(x => x.Id);

            //builder.EntitySet<Optionend>("OptionExtendTypes");

            var organizationUnitRights = builder.EntitySet<OrganizationUnitRight>(nameof(OrganizationUnitRightsController).Replace("Controller", string.Empty));
            organizationUnitRights.EntityType.HasKey(x => x.Id);

            var organiationUnitRoles = builder.EntitySet<OrganizationUnitRole>(nameof(OrganizationUnitRolesController).Replace("Controller", string.Empty));
            organiationUnitRoles.EntityType.HasKey(x => x.Id);

            //builder.EntitySet<PasswordResetRequest>("PasswordResetRequests");
            //builder.EntitySet<PaymentFreqencyType>("PaymentFreqencyTypes");
            //builder.EntitySet<PaymentMilestone>("PaymentMilestones");
            //builder.EntitySet<PaymentModelType>("PaymentModelTypes");
            //builder.EntitySet<PriceRegulationType>("PriceRegulationTypes");
            //builder.EntitySet<ProcurementStrategyType>("ProcurementStrategyTypes");
            builder.EntitySet<ItProjectType>("ItProjectTypes"); // no controller yet
            //builder.EntitySet<PurchaseFormType>("PurchaseFormTypes");
            //builder.EntitySet<Risk>("Risks");
            //builder.EntitySet<Stakeholder>("Stakeholders");
            //builder.EntitySet<TerminationDeadlineType>("TerminationDeadlineTypes");
            //builder.EntitySet<TaskRef>("TaskRefs");
            //builder.EntitySet<TaskUsage>("TaskUsages");
            //builder.EntitySet<Text>("Texts");
            //builder.EntitySet<User>("Users");
            //builder.EntitySet<Wish>("Wishes");

            builder.EntitySet<Report>("Reports").EntityType.HasKey(x => x.Id);
            builder.EntitySet<ReportCategoryType>("ReportCategories").EntityType.HasKey(x => x.Id);

            return builder.GetEdmModel();
        }
    }
}
