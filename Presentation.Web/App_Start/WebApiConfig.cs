using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using Core.DomainModel;
using Core.DomainModel.ItContract;
using Core.DomainModel.ItSystem;
using Core.DomainModel.ItSystemUsage;
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

            var archiveOption = builder.EntitySet<ArchiveType>("ArchiveTypes");
            archiveOption.EntityType.HasKey(x => x.Id);
            archiveOption.EntityType.Property(x => x.Name);

            var itSystems = builder.EntitySet<ItSystem>("ItSystems");
            itSystems.EntityType.HasKey(x => x.Id);
            itSystems.EntityType.Property(x => x.Name);
            itSystems.EntityType.Property(x => x.Description);
            itSystems.EntityType.Property(x => x.ParentId);
            itSystems.EntityType.HasOptional(x => x.Parent).IsNavigable();
            itSystems.EntityType.HasMany(x => x.Children).NonContained().IsNavigable();
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
            itSystems.EntityType.HasMany(x => x.ItInterfaceExhibits).IsNavigable();
            itSystems.EntityType.HasMany(x => x.CanUseInterfaces).IsNavigable();
            itSystems.EntityType.Property(x => x.Url);
            itSystems.EntityType.Property(x => x.LastChanged);
            itSystems.EntityType.HasRequired(x => x.LastChangedByUser);

            var itSystemTypeOptions = builder.EntitySet<ItSystemTypeOption>("ItSystemTypeOptions");
            itSystemTypeOptions.EntityType.HasKey(x => x.Id);
            itSystemTypeOptions.EntityType.Property(x => x.Name);

            var businessTypes = builder.EntitySet<BusinessType>("BusinessTypes");
            businessTypes.EntityType.HasKey(x => x.Id);
            businessTypes.EntityType.Property(x => x.Name);

            var taskRefs = builder.EntitySet<TaskRef>("TaskRefs");
            taskRefs.EntityType.HasKey(x => x.Id);
            taskRefs.EntityType.Property(x => x.TaskKey);
            taskRefs.EntityType.Property(x => x.Description);

            var organizations = builder.EntitySet<Organization>("Organizations");
            organizations.EntityType.HasKey(x => x.Id);
            organizations.EntityType.Property(x => x.Name);
            organizations.EntityType.HasMany(x => x.ItSystems).IsNavigable();
            organizations.EntityType.HasMany(x => x.ItSystemUsages).IsNavigable();
            organizations.EntityType.HasMany(x => x.ItInterfaces).IsNavigable();
            organizations.EntityType.HasMany(x => x.OrgUnits).IsNavigable().Name = "OrganizationUnits";

            var orgUnits = builder.EntitySet<OrganizationUnit>("OrganizationUnits");
            orgUnits.EntityType.HasKey(x => x.Id);
            orgUnits.EntityType.Property(x => x.Name);
            orgUnits.EntityType.Property(x => x.ParentId);
            orgUnits.EntityType.HasOptional(x => x.Parent).IsNavigable();
            orgUnits.EntityType.HasMany(x => x.Children).IsNavigable();

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
            usages.EntityType.Property(x => x.LastChanged);
            usages.EntityType.Property(x => x.LocalSystemId);
            usages.EntityType.HasOptional(x => x.ResponsibleUsage);
            usages.EntityType.HasOptional(x => x.MainContract);
            usages.EntityType.Property(x => x.OverviewId);
            usages.EntityType.HasOptional(x => x.Overview);
            usages.EntityType.HasMany(x => x.Rights).IsNavigable();
            usages.EntityType.Property(x => x.EsdhRef);
            usages.EntityType.Property(x => x.CmdbRef);
            usages.EntityType.Property(x => x.DirectoryOrUrlRef);
            usages.EntityType.HasOptional(x => x.SensitiveDataType);
            usages.EntityType.HasOptional(x => x.ArchiveType);
            usages.EntityType.HasRequired(x => x.LastChangedByUser);
            usages.EntityType.HasRequired(x => x.ObjectOwner);

            var itSystemRights = builder.EntitySet<ItSystemRight>("ItSystemRights");
            itSystemRights.EntityType.HasKey(x => x.Id);
            itSystemRights.EntityType.Property(x => x.RoleId);
            itSystemRights.EntityType.HasOptional(x => x.Role).IsNavigable();
            itSystemRights.EntityType.Property(x => x.ObjectId);
            itSystemRights.EntityType.HasOptional(x => x.Object).IsNavigable();
            itSystemRights.EntityType.Property(x => x.UserId);
            itSystemRights.EntityType.HasOptional(x => x.User).IsNavigable();
            itSystemRights.EntityType.Property(x => x.ObjectOwnerId);
            itSystemRights.EntityType.HasOptional(x => x.ObjectOwner).IsNavigable();

            var roles = builder.EntitySet<ItSystemRole>("ItSystemRoles");
            roles.EntityType.HasKey(x => x.Id);
            roles.EntityType.Property(x => x.Name);

            var systemOrgUnitUsages = builder.EntitySet<ItSystemUsageOrgUnitUsage>("ItSystemUsageOrgUnitUsages");
            systemOrgUnitUsages.EntityType.HasKey(x => x.ItSystemUsageId).HasKey(x => x.OrganizationUnitId);
            systemOrgUnitUsages.EntityType.HasOptional(x => x.OrganizationUnit);

            var contractItSystemUsages = builder.EntitySet<ItContractItSystemUsage>("ItContractItSystemUsages");
            contractItSystemUsages.EntityType.HasKey(x => x.ItContractId).HasKey(x => x.ItSystemUsageId);
            contractItSystemUsages.EntityType.HasOptional(x => x.ItContract);

            var contracts = builder.EntitySet<ItContract>("ItContracts");
            contracts.EntityType.HasKey(x => x.Id);

            // TODO this field is causing issues.
            // This query fails: /odata/Organizations(1)/ItSystemUsages?$expand=MainContract($expand=ItContract)
            // if ItContract.Terminated has a value
            contracts.EntityType.Property(x => x.IsActive);

            var interfaces = builder.EntitySet<Interface>("Interfaces");
            interfaces.EntityType.HasKey(x => x.Id);
            interfaces.EntityType.Property(x => x.Name);

            var itInterfaces = builder.EntitySet<ItInterface>("ItInterfaces");
            itInterfaces.EntityType.HasKey(x => x.Id);
            itInterfaces.EntityType.Property(x => x.Name);
            itInterfaces.EntityType.EnumProperty(x => x.AccessModifier);
            itInterfaces.EntityType.Property(x => x.BelongsToId);
            itInterfaces.EntityType.HasOptional(x => x.BelongsTo);
            itInterfaces.EntityType.Property(x => x.InterfaceId);
            itInterfaces.EntityType.HasOptional(x => x.Interface);
            itInterfaces.EntityType.Property(x => x.InterfaceTypeId);
            itInterfaces.EntityType.HasOptional(x => x.InterfaceType);
            itInterfaces.EntityType.HasRequired(x => x.ObjectOwner);
            itInterfaces.EntityType.Property(x => x.OrganizationId);
            itInterfaces.EntityType.HasRequired(x => x.Organization).IsNavigable();
            itInterfaces.EntityType.HasOptional(x => x.Tsa);
            itInterfaces.EntityType.HasOptional(x => x.Method);
            itInterfaces.EntityType.HasOptional(x => x.ExhibitedBy);
            itInterfaces.EntityType.Property(x => x.ItInterfaceId);
            itInterfaces.EntityType.Property(x => x.Version);
            itInterfaces.EntityType.Property(x => x.Url);
            itInterfaces.EntityType.Property(x => x.LastChanged);
            itInterfaces.EntityType.HasRequired(x => x.LastChangedByUser);

            var interfaceTypes = builder.EntitySet<InterfaceType>("InterfaceType");
            interfaceTypes.EntityType.HasKey(x => x.Id);
            interfaceTypes.EntityType.Property(x => x.Name);

            var itInterfaceExihibits = builder.EntitySet<ItInterfaceExhibit>("ItInterfaceExhibits");
            itInterfaceExihibits.EntityType.HasKey(x => x.Id);
            itInterfaceExihibits.EntityType.Property(x => x.ItSystemId);
            itInterfaceExihibits.EntityType.HasRequired(x => x.ItSystem);
            itInterfaceExihibits.EntityType.HasOptional(x => x.ItInterface);

            var itInterfaceExhibitUsage = builder.EntitySet<ItInterfaceExhibitUsage>("ItInterfaceExhibitUsages");
            itInterfaceExhibitUsage.EntityType.HasKey(x => x.ItContractId)
                .HasKey(x => x.ItInterfaceExhibitId)
                .HasKey(x => x.ItSystemUsageId);

            var itInterfaceUse = builder.EntitySet<ItInterfaceUse>("ItInterfaceUses");
            itInterfaceUse.EntityType
                .HasKey(x => x.ItSystemId)
                .HasKey(x => x.ItInterfaceId);
            itInterfaceUse.EntityType.HasRequired(x => x.ItInterface);
            itInterfaceUse.EntityType.HasRequired(x => x.ItSystem);

            var tsas = builder.EntitySet<Tsa>("Tsas");
            tsas.EntityType.HasKey(x => x.Id);
            tsas.EntityType.Property(x => x.Name);

            var methods = builder.EntitySet<Method>("Methods");
            methods.EntityType.HasKey(x => x.Id);
            methods.EntityType.Property(x => x.Name);

            var sensitiveDataOption = builder.EntitySet<SensitiveDataType>("SensitiveDataTypes");
            sensitiveDataOption.EntityType.HasKey(x => x.Id);
            sensitiveDataOption.EntityType.Property(x => x.Name);

            //builder.EntitySet<ItSystemTypeOption>("ItSystemTypeOptions");
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
