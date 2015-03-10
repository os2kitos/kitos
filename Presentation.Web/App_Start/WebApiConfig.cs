using System;
using System.EnterpriseServices;
using System.Linq;
using System.Web.Compilation;
using System.Web.Http;
using System.Web.Http.OData.Routing;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using Core.ApplicationServices;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItProject;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
using Infrastructure.DataAccess;
using Microsoft.Ajax.Utilities;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Presentation.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
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


            config.EnableCaseInsensitive(true);

            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }

        public static IEdmModel GetModel()
        {
            var builder = new ODataModelBuilder();

            builder.AddEnumType(typeof(AccessModifier));

            //builder.EntitySet<AdminRight>("AdminRights");
            //builder.EntitySet<AdminRole>("AdminRoles");
            //builder.EntitySet<Advice>("Advices");
            //builder.EntitySet<AgreementElement>("AgreementElements");
            //builder.EntitySet<ArchiveType>("ArchiveTypes");
            //builder.EntitySet<BusinessType>("BusinessTypes");
            //builder.EntitySet<Communication>("Communications");
            //builder.EntitySet<Config>("Configs");
            //builder.EntitySet<ContractTemplate>("ContractTemplates");
            //builder.EntitySet<ContractType>("ContractTypes");
            //builder.EntitySet<DataType>("DataTypes");
            //builder.EntitySet<DataRow>("DataRows");
            //builder.EntitySet<DataRowUsage>("DataRowUsages");
            //builder.EntitySet<EconomyYear>("EconomyYears");
            //builder.EntitySet<EconomyStream>("EconomyStrams");
            //builder.EntitySet<Frequency>("Frequencies");
            //builder.EntitySet<Goal>("Goals");
            //builder.EntitySet<GoalStatus>("GoalStatus");
            //builder.EntitySet<GoalType>("GoalTypes");
            //builder.EntitySet<Handover>("Handovers");
            //builder.EntitySet<HandoverTrial>("HandoverTrials");
            //builder.EntitySet<HandoverTrialType>("HandoverTrialTypes");
            //builder.EntitySet<Interface>("Interfaces");
            //builder.EntitySet<ItInterfaceUse>("ItInterfaceUses");
            //builder.EntitySet<InterfaceUsage>("InterfaceUsages");
            //builder.EntitySet<ItInterfaceExhibit>("ItInterfaceExhibits");
            //builder.EntitySet<ItInterfaceExhibitUsage>("InterfaceExhibtUsages");
            //builder.EntitySet<InterfaceType>("InterfaceTypes");
            //builder.EntitySet<ItContract>("ItContracts");
            //builder.EntitySet<ItContractItSystemUsage>("ItContractItSystemUsages");
            //builder.EntitySet<ItContractRight>("ItContractRights");
            //builder.EntitySet<ItContractRole>("ItContractRoles");
            //builder.EntitySet<ItProject>("ItProjects");
            //builder.EntitySet<ItProjectPhase>("ItProjectPhases");
            //builder.EntitySet<ItProjectStatus>("ItProjectStatuses");
            //builder.EntitySet<ItProjectRight>("ItProjectRights");
            //builder.EntitySet<ItProjectRole>("ItProjectRoles");
            //builder.EntitySet<ItProjectOrgUnitUsage>("ItProjectOrgUnitUsages");
            //builder.EntitySet<ItSystemUsageOrgUnitUsage>("ItSystemUsageOrgUnitUsages");
            //builder.EntitySet<ItSystem>("ItSystems").EntityType.HasKey(x => x.Id);


            var itSystems = builder.EntitySet<ItSystem>("ItSystems");
            itSystems.EntityType.HasKey(x => x.Id);
            itSystems.EntityType.Property(x => x.Name);
            itSystems.EntityType.Property(x => x.Description);
            itSystems.EntityType.Property(x => x.ParentId);
            itSystems.EntityType.HasOptional(x => x.Parent).IsNavigable();
            itSystems.EntityType.EnumProperty(x => x.AccessModifier);
            itSystems.EntityType.Property(x => x.AppTypeOptionId);
            itSystems.EntityType.HasOptional(x => x.AppTypeOption).IsNavigable();
            itSystems.EntityType.Property(x => x.BusinessTypeId);
            itSystems.EntityType.HasOptional(x => x.BusinessType).IsNavigable();
            itSystems.EntityType.HasMany(x => x.TaskRefs).IsNavigable();
            itSystems.EntityType.Property(x => x.BelongsToId);
            itSystems.EntityType.HasOptional(x => x.BelongsTo).IsNavigable();
            itSystems.EntityType.Property(x => x.OrganizationId);
            itSystems.EntityType.HasOptional(x => x.Organization).IsNavigable();
            itSystems.EntityType.Property(x => x.ObjectOwnerId);
            itSystems.EntityType.HasOptional(x => x.ObjectOwner).IsNavigable();
            itSystems.EntityType.HasMany(x => x.Usages).IsNavigable();

            var itSystemTypeOptions = builder.EntitySet<ItSystemTypeOption>("ItSystemTypeOptions");
            itSystemTypeOptions.EntityType.HasKey(x => x.Id);
            itSystemTypeOptions.EntityType.Property(x => x.Name);

            var businessTypes = builder.EntitySet<BusinessType>("BusinessTypes");
            businessTypes.EntityType.HasKey(x => x.Id);
            businessTypes.EntityType.Property(x => x.Name);

            var taskRefs = builder.EntitySet<TaskRef>("TaskRefs");
            taskRefs.EntityType.HasKey(x => x.Id);
            taskRefs.EntityType.Property(x => x.TaskKey);

            var organizations = builder.EntitySet<Organization>("Organizations");
            organizations.EntityType.HasKey(x => x.Id);
            organizations.EntityType.Property(x => x.Name);

            var orgUnits = builder.EntitySet<OrganizationUnit>("OrganizationUnits");
            orgUnits.EntityType.HasKey(x => x.Id);
            orgUnits.EntityType.Property(x => x.Name);

            var users = builder.EntitySet<User>("Users");
            users.EntityType.HasKey(x => x.Id);
            users.EntityType.Property(x => x.Name);
            users.EntityType.Property(x => x.LastName);

            var usages = builder.EntitySet<ItSystemUsage>("ItSystemUsages");
            usages.EntityType.HasKey(x => x.Id);
            usages.EntityType.HasRequired(x => x.ItSystem).IsNavigable();
            usages.EntityType.Property(x => x.OrganizationId);
            usages.EntityType.HasOptional(x => x.Organization).IsNavigable();
            usages.EntityType.Property(x => x.ItSystemId);
            usages.EntityType.HasOptional(x => x.ResponsibleUsage);
            usages.EntityType.HasOptional(x => x.MainContract);
            usages.EntityType.Property(x => x.OverviewId);
            usages.EntityType.HasOptional(x => x.Overview);

            var systemOrgUnitUsages = builder.EntitySet<ItSystemUsageOrgUnitUsage>("ItSystemUsageOrgUnitUsages");
            systemOrgUnitUsages.EntityType.HasKey(x => x.ItSystemUsageId).HasKey(x => x.OrganizationUnitId);
            systemOrgUnitUsages.EntityType.HasOptional(x => x.OrganizationUnit);

            var contractItSystemUsages = builder.EntitySet<ItContractItSystemUsage>("ItContractItSystemUsages");
            contractItSystemUsages.EntityType.HasKey(x => x.ItContractId).HasKey(x => x.ItSystemUsageId);
            contractItSystemUsages.EntityType.HasOptional(x => x.ItContract);

            var contracts = builder.EntitySet<ItContract>("ItContracts");
            contracts.EntityType.HasKey(x => x.Id);
            contracts.EntityType.Property(x => x.IsActive);

            var interfaces = builder.EntitySet<Interface>("Interfaces");
            interfaces.EntityType.HasKey(x => x.Id);
            interfaces.EntityType.Property(x => x.Name);
            interfaces.EntityType.ComplexProperty(x => x.References);

            var itInterfaces = builder.EntitySet<ItInterface>("ItInterfaces");
            itInterfaces.EntityType.HasKey(x => x.Id);
            itInterfaces.EntityType.Property(x => x.Name);
            itInterfaces.EntityType.EnumProperty(x => x.AccessModifier);
            itInterfaces.EntityType.Property(x => x.BelongsToId);
            itInterfaces.EntityType.HasOptional(x => x.BelongsTo);

            //builder.EntitySet<ItSystemUsage>("ItSystemUsages");
            //builder.EntitySet<ItSystemRight>("ItSystemRights");
            //builder.EntitySet<ItSystemRole>("ItSystemRoles");
            //builder.EntitySet<ItSystemTypeOption>("ItSystemTypeOptions");
            //builder.EntitySet<Method>("Methods");
            //builder.EntitySet<OptionExtend>("OptionExtention");
            //builder.EntitySet<Organization>("Organizations");
            //builder.EntitySet<OrganizationUnit>("OrganizationUnits");
            //builder.EntitySet<OrganizationRight>("OrganizationRights");
            //builder.EntitySet<OrganizationRole>("OrganizationRoles");
            //builder.EntitySet<PasswordResetRequest>("PasswordResetRequests");
            //builder.EntitySet<PaymentFreqency>("PaymentFreqencies");
            //builder.EntitySet<PaymentMilestone>("PaymentMilestones");
            //builder.EntitySet<PaymentModel>("PaymentModels");
            //builder.EntitySet<PriceRegulation>("PriceRegulations");
            //builder.EntitySet<ProcurementStrategy>("ProcurementStrategies");
            //builder.EntitySet<ItProjectType>("ProjectTypes");
            //builder.EntitySet<PurchaseForm>("PurchaseForms");
            //builder.EntitySet<Risk>("Risks");
            //builder.EntitySet<SensitiveDataType>("SensitiveDataTypes");
            //builder.EntitySet<Stakeholder>("Stakeholders");
            //builder.EntitySet<TerminationDeadline>("TerminationDeadlines");
            //builder.EntitySet<TaskRef>("TaskRefs");
            //builder.EntitySet<TaskUsage>("TaskUsages");
            //builder.EntitySet<Text>("Texts");
            //builder.EntitySet<Tsa>("Tsas");
            //builder.EntitySet<User>("Users");
            //builder.EntitySet<Wish>("Wishes");

            return builder.GetEdmModel();
        }
    }
}
