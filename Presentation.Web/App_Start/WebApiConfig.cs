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
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }

        public static IEdmModel GetModel()
        {
            var builder = new ODataConventionModelBuilder();

            var accessMod = builder.AddEnumType(typeof(AccessModifier));
            accessMod.Namespace = "Kitos";

            //builder.EntitySet<OrganizationRight>("OrganizationRights");
            //builder.EntitySet<OrganizationRole>("OrganizationRoles");
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
            //builder.EntitySet<ItContractRight>("ItContractRights");
            //builder.EntitySet<ItSystemUsageOrgUnitUsage>("ItSystemUsageOrgUnitUsages");

            var itContractRoles = builder.EntitySet<ItContractRole>("ItContractRoles");
            itContractRoles.EntityType.HasKey(x => x.Id);

            //builder.EntitySet<ItProjectStatus>("ItProjectStatuses");

            var itProjectRights = builder.EntitySet<ItProjectRight>("ItProjectRights");
            itProjectRights.EntityType.HasKey(x => x.Id);

            var itProjectRoles = builder.EntitySet<ItProjectRole>("ItProjectRoles");
            itProjectRoles.EntityType.HasKey(x => x.Id);

            var itProjectOrgUnitUsage = builder.EntitySet<ItProjectOrgUnitUsage>("ItProjectOrgUnitUsages");
            itProjectOrgUnitUsage.EntityType.HasKey(x => new { x.ItProjectId, x.OrganizationUnitId });

            var itProject = builder.EntitySet<ItProject>("ItProjects");
            itProject.EntityType.HasKey(x => x.Id);

            var interfaceUsage = builder.EntitySet<ItInterfaceUsage>("ItInterfaceUsages");
            interfaceUsage.EntityType.HasKey(x => new { x.ItSystemUsageId, x.ItSystemId, x.ItInterfaceId });

            var dataOption = builder.EntitySet<DataType>("DataTypes");
            dataOption.EntityType.HasKey(x => x.Id);

            var dataRow = builder.EntitySet<DataRow>("DataRows");
            dataRow.EntityType.HasKey(x => x.Id);

            var archiveOption = builder.EntitySet<ArchiveType>("ArchiveTypes");
            archiveOption.EntityType.HasKey(x => x.Id);

            var itSystems = builder.EntitySet<ItSystem>("ItSystems");
            itSystems.EntityType.HasKey(x => x.Id);

            var itSystemTypeOptions = builder.EntitySet<ItSystemType>("ItSystemTypes");
            itSystemTypeOptions.EntityType.HasKey(x => x.Id);

            var businessTypes = builder.EntitySet<BusinessType>("BusinessTypes");
            businessTypes.EntityType.HasKey(x => x.Id);

            var taskRefs = builder.EntitySet<TaskRef>("TaskRefs");
            taskRefs.EntityType.HasKey(x => x.Id);

            var organizations = builder.EntitySet<Organization>("Organizations");
            organizations.EntityType.HasKey(x => x.Id);
            organizations.EntityType.HasMany(x => x.OrgUnits).IsNavigable().Name = "OrganizationUnits";

            var orgUnits = builder.EntitySet<OrganizationUnit>("OrganizationUnits");
            orgUnits.EntityType.HasKey(x => x.Id);
            orgUnits.EntityType.HasMany(x => x.ResponsibleForItContracts).Name = "ItContracts";
            orgUnits.EntityType.HasMany(x => x.UsingItProjects).Name = "ItProjects";

            var users = builder.EntitySet<User>("Users");
            users.EntityType.HasKey(x => x.Id);
            users.EntityType.Ignore(x => x.Email);
            users.EntityType.Ignore(x => x.Password);
            users.EntityType.Ignore(x => x.Salt);
            users.EntityType.Ignore(x => x.PhoneNumber);

            var usages = builder.EntitySet<ItSystemUsage>("ItSystemUsages");
            usages.EntityType.HasKey(x => x.Id);

            var itSystemRights = builder.EntitySet<ItSystemRight>("ItSystemRights");
            itSystemRights.EntityType.HasKey(x => x.Id);

            var roles = builder.EntitySet<ItSystemRole>("ItSystemRoles");
            roles.EntityType.HasKey(x => x.Id);

            var systemOrgUnitUsages = builder.EntitySet<ItSystemUsageOrgUnitUsage>("ItSystemUsageOrgUnitUsages");
            systemOrgUnitUsages.EntityType.HasKey(x => x.ItSystemUsageId).HasKey(x => x.OrganizationUnitId);

            var contractItSystemUsages = builder.EntitySet<ItContractItSystemUsage>("ItContractItSystemUsages");
            contractItSystemUsages.EntityType.HasKey(x => x.ItContractId).HasKey(x => x.ItSystemUsageId);

            var contracts = builder.EntitySet<ItContract>("ItContracts");
            contracts.EntityType.HasKey(x => x.Id);
            contracts.EntityType.HasMany(x => x.ExternEconomyStreams).IsNotExpandable(); // do not remove
            contracts.EntityType.HasMany(x => x.InternEconomyStreams).IsNotExpandable(); // do not remove

            // TODO this field is causing issues.
            // This query fails: /odata/Organizations(1)/ItSystemUsages?$expand=MainContract($expand=ItContract)
            // if ItContract.Terminated has a value
            contracts.EntityType.Ignore(x => x.IsActive);

            var interfaceTypes = builder.EntitySet<InterfaceType>("InterfaceTypes");
            interfaceTypes.EntityType.HasKey(x => x.Id);

            var itInterfaces = builder.EntitySet<ItInterface>("ItInterfaces");
            itInterfaces.EntityType.HasKey(x => x.Id);

            var itInterfaceTypes = builder.EntitySet<ItInterfaceType>("ItInterfaceTypes");
            itInterfaceTypes.EntityType.HasKey(x => x.Id);

            var itInterfaceExihibits = builder.EntitySet<ItInterfaceExhibit>("ItInterfaceExhibits");
            itInterfaceExihibits.EntityType.HasKey(x => x.Id);

            var itInterfaceExhibitUsage = builder.EntitySet<ItInterfaceExhibitUsage>("ItInterfaceExhibitUsages");
            itInterfaceExhibitUsage.EntityType.HasKey(x => x.ItContractId)
                .HasKey(x => x.ItInterfaceExhibitId)
                .HasKey(x => x.ItSystemUsageId);

            var itInterfaceUse = builder.EntitySet<ItInterfaceUse>("ItInterfaceUses");
            itInterfaceUse.EntityType
                .HasKey(x => x.ItSystemId)
                .HasKey(x => x.ItInterfaceId);

            var tsas = builder.EntitySet<TsaType>("TsaTypes");
            tsas.EntityType.HasKey(x => x.Id);

            var methods = builder.EntitySet<MethodType>("MethodTypes");
            methods.EntityType.HasKey(x => x.Id);

            var sensitiveDataOption = builder.EntitySet<SensitiveDataType>("SensitiveDataTypes");
            sensitiveDataOption.EntityType.HasKey(x => x.Id);

            //builder.EntitySet<Optionend>("OptionExtendTypes");
            //builder.EntitySet<OrganizationUnitRight>("OrganizationUnitRights");
            //builder.EntitySet<OrganizationUnitRole>("OrganizationUnitRoles");
            //builder.EntitySet<PasswordResetRequest>("PasswordResetRequests");
            //builder.EntitySet<PaymentFreqencyType>("PaymentFreqencyTypes");
            //builder.EntitySet<PaymentMilestone>("PaymentMilestones");
            //builder.EntitySet<PaymentModelType>("PaymentModelTypes");
            //builder.EntitySet<PriceRegulationType>("PriceRegulationTypes");
            //builder.EntitySet<ProcurementStrategyType>("ProcurementStrategyTypes");
            builder.EntitySet<ItProjectType>("ItProjectTypes");
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
