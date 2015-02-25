using System;
using System.EnterpriseServices;
using System.Linq;
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
            var builder = new ODataModelBuilder();

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

            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            config.MapODataServiceRoute(
                routeName: "odata",
                routePrefix: "odata",
                model: builder.GetEdmModel());
        }
    }
}
