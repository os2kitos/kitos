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
            config.Filters.Add(new ExceptionLogFilterAttribute());
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

            var agreementElementTypes = builder.EntitySet<AgreementElementType>(nameof(AgreementElementTypesController).Replace("Controller", string.Empty));
            agreementElementTypes.EntityType.HasKey(x => x.Id);

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

            var itProjectOrgUnitUsage = builder.EntitySet<ItProjectOrgUnitUsage>("ItProjectOrgUnitUsages"); // no controller yet
            itProjectOrgUnitUsage.EntityType.HasKey(x => new {x.ItProjectId, x.OrganizationUnitId});

            var itProject = builder.EntitySet<ItProject>(nameof(ItProjectsController).Replace("Controller", string.Empty));
            itProject.EntityType.HasKey(x => x.Id);

            var interfaceUsage = builder.EntitySet<ItInterfaceUsage>("ItInterfaceUsages"); // no controller yet
            interfaceUsage.EntityType.HasKey(x => new { x.ItSystemUsageId, x.ItSystemId, x.ItInterfaceId });

            var dataOption = builder.EntitySet<DataType>(nameof(DataTypesController).Replace("Controller", string.Empty));
            dataOption.EntityType.HasKey(x => x.Id);

            var dataRow = builder.EntitySet<DataRow>("DataRows"); // no controller yet
            dataRow.EntityType.HasKey(x => x.Id);

            var archiveOption = builder.EntitySet<ArchiveType>(nameof(ArchiveTypesController).Replace("Controller", string.Empty));
            archiveOption.EntityType.HasKey(x => x.Id);

            var itSystems = builder.EntitySet<ItSystem>(nameof(ItSystemsController).Replace("Controller", string.Empty));
            itSystems.EntityType.HasKey(x => x.Id);

            var itSystemType = builder.EntitySet<ItSystemType>(nameof(ItSystemTypesController).Replace("Controller", string.Empty));
            itSystemType.EntityType.HasKey(x => x.Id);

            var businessTypes = builder.EntitySet<BusinessType>(nameof(BusinessTypesController).Replace("Controller", string.Empty));
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

            var interfaceTypes = builder.EntitySet<InterfaceType>(nameof(InterfaceTypesController).Replace("Controller", string.Empty));
            interfaceTypes.EntityType.HasKey(x => x.Id);

            var itInterfaces = builder.EntitySet<ItInterface>(nameof(ItInterfacesController).Replace("Controller", string.Empty));
            itInterfaces.EntityType.HasKey(x => x.Id);

            var itInterfaceTypes = builder.EntitySet<ItInterfaceType>(nameof(ItInterfaceTypesController).Replace("Controller", string.Empty));
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

            var tsas = builder.EntitySet<TsaType>(nameof(TsaTypesController).Replace("Controller", string.Empty));
            tsas.EntityType.HasKey(x => x.Id);

            var methodTypes = builder.EntitySet<MethodType>(nameof(MethodTypesController).Replace("Controller", string.Empty));
            methodTypes.EntityType.HasKey(x => x.Id);

            var sensitiveDataOption = builder.EntitySet<SensitiveDataType>(nameof(SensitiveDataTypesController).Replace("Controller", string.Empty));
            sensitiveDataOption.EntityType.HasKey(x => x.Id);

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
            LocalAgreementElementType.EntityType.HasKey(x => x.Id);

            var LocalArchiveType = builder.EntitySet<LocalArchiveType>(nameof(LocalArchiveTypesController).Replace("Controller", string.Empty));
            LocalArchiveType.EntityType.HasKey(x => x.Id);

            var LocalBusinessType = builder.EntitySet<LocalBusinessType>(nameof(LocalBusinessTypesController).Replace("Controller", string.Empty));
            LocalBusinessType.EntityType.HasKey(x => x.Id);

            var LocalDataType = builder.EntitySet<LocalDataType>(nameof(LocalDataTypesController).Replace("Controller", string.Empty));
            LocalDataType.EntityType.HasKey(x => x.Id);

            var LocalFrequencyType = builder.EntitySet<LocalFrequencyType>(nameof(LocalFrequencyTypesController).Replace("Controller", string.Empty));
            LocalFrequencyType.EntityType.HasKey(x => x.Id);

            var LocalGoalType = builder.EntitySet<LocalGoalType>(nameof(LocalGoalTypesController).Replace("Controller", string.Empty));
            LocalGoalType.EntityType.HasKey(x => x.Id);

            var LocalHandoverTrialType = builder.EntitySet<LocalHandoverTrialType>(nameof(LocalHandoverTrialTypesController).Replace("Controller", string.Empty));
            LocalHandoverTrialType.EntityType.HasKey(x => x.Id);

            var LocalInterfaceType = builder.EntitySet<LocalInterfaceType>(nameof(LocalInterfaceTypesController).Replace("Controller", string.Empty));
            LocalInterfaceType.EntityType.HasKey(x => x.Id);

            var LocalItContractRole = builder.EntitySet<LocalItContractRole>(nameof(LocalItContractRolesController).Replace("Controller", string.Empty));
            LocalItContractRole.EntityType.HasKey(x => x.Id);

            var LocalItContractTemplateType = builder.EntitySet<LocalItContractTemplateType>(nameof(LocalItContractTemplateTypesController).Replace("Controller", string.Empty));
            LocalItContractTemplateType.EntityType.HasKey(x => x.Id);

            var LocalItContractType = builder.EntitySet<LocalItContractType>(nameof(LocalItContractTypesController).Replace("Controller", string.Empty));
            LocalItContractType.EntityType.HasKey(x => x.Id);

            var LocalItInterfaceType = builder.EntitySet<LocalItInterfaceType>(nameof(LocalItInterfaceTypesController).Replace("Controller", string.Empty));
            LocalItInterfaceType.EntityType.HasKey(x => x.Id);

            var LocalItProjectRole = builder.EntitySet<LocalItProjectRole>(nameof(LocalItProjectRolesController).Replace("Controller", string.Empty));
            LocalItProjectRole.EntityType.HasKey(x => x.Id);

            var LocalItProjectType = builder.EntitySet<LocalItProjectType>(nameof(LocalItProjectTypesController).Replace("Controller", string.Empty));
            LocalItProjectType.EntityType.HasKey(x => x.Id);

            var LocalItSystemRole = builder.EntitySet<LocalItSystemRole>(nameof(LocalItSystemRolesController).Replace("Controller", string.Empty));
            LocalItSystemRole.EntityType.HasKey(x => x.Id);

            var LocalItSystemType = builder.EntitySet<LocalItSystemType>(nameof(LocalItSystemTypesController).Replace("Controller", string.Empty));
            LocalItSystemType.EntityType.HasKey(x => x.Id);

            var LocalMethodType = builder.EntitySet<LocalMethodType>(nameof(LocalMethodTypesController).Replace("Controller", string.Empty));
            LocalMethodType.EntityType.HasKey(x => x.Id);

            var LocalOptionExtendType = builder.EntitySet<LocalOptionExtendType>(nameof(LocalOptionExtendTypesController).Replace("Controller", string.Empty));
            LocalOptionExtendType.EntityType.HasKey(x => x.Id);

            var LocalPaymentFreqencyType = builder.EntitySet<LocalPaymentFreqencyType>(nameof(LocalPaymentFrequencyTypesController).Replace("Controller", string.Empty));
            LocalPaymentFreqencyType.EntityType.HasKey(x => x.Id);

            var LocalPaymentModelType = builder.EntitySet<LocalPaymentModelType>(nameof(LocalPaymentModelTypesController).Replace("Controller", string.Empty));
            LocalPaymentModelType.EntityType.HasKey(x => x.Id);

            var LocalPriceRegulationType = builder.EntitySet<LocalPriceRegulationType>(nameof(LocalPriceRegulationTypesController).Replace("Controller", string.Empty));
            LocalPriceRegulationType.EntityType.HasKey(x => x.Id);

            var LocalProcurementStrategyType = builder.EntitySet<LocalProcurementStrategyType>(nameof(LocalProcurementStrategyTypesController).Replace("Controller", string.Empty));
            LocalProcurementStrategyType.EntityType.HasKey(x => x.Id);

            var LocalPurchaseFormType = builder.EntitySet<LocalPurchaseFormType>(nameof(LocalPurchaseFormTypesController).Replace("Controller", string.Empty));
            LocalPurchaseFormType.EntityType.HasKey(x => x.Id);

            var LocalReportCategoryType = builder.EntitySet<LocalReportCategoryType>(nameof(LocalReportCategoryTypesController).Replace("Controller", string.Empty));
            LocalReportCategoryType.EntityType.HasKey(x => x.Id);

            var LocalSensitiveDataType = builder.EntitySet<LocalSensitiveDataType>(nameof(LocalSensitiveDataTypesController).Replace("Controller", string.Empty));
            LocalSensitiveDataType.EntityType.HasKey(x => x.Id);

            var LocalTerminationDeadlineType = builder.EntitySet<LocalTerminationDeadlineType>(nameof(LocalTerminationDeadlineTypesController).Replace("Controller", string.Empty));
            LocalTerminationDeadlineType.EntityType.HasKey(x => x.Id);

            var LocalTsaType = builder.EntitySet<LocalTsaType>(nameof(LocalTsaTypesController).Replace("Controller", string.Empty));
            LocalTsaType.EntityType.HasKey(x => x.Id);

            //builder.EntitySet<Risk>("Risks");
            //builder.EntitySet<Stakeholder>("Stakeholders");

            var terminationDeadlineType = builder.EntitySet<TerminationDeadlineType>(nameof(TerminationDeadlineTypesController).Replace("Controller", string.Empty));
            terminationDeadlineType.EntityType.HasKey(x => x.Id);

            var config = builder.EntitySet<Config>(nameof(ConfigsController).Replace("Controller", string.Empty));
            config.EntityType.HasKey(x => x.Id);


            var Advice = builder.EntitySet<Advice>(nameof(AdvisController).Replace("Controller", string.Empty));
            Advice.EntityType.HasKey(x => x.Id);

            var adviceSent = builder.EntitySet<AdviceSent>(nameof(AdviceSentController).Replace("Controller", string.Empty));
            adviceSent.EntityType.HasKey(x => x.Id);
            // var GetByObjectId = users.EntityType.Collection.Function("GetByObjectId").Returns<Icolle>();

            var GetAdvicesByObjectID = builder.Function("GetAdvicesByObjectID");
            GetAdvicesByObjectID.Parameter<int>("id");
            GetAdvicesByObjectID.Parameter<int>("type");
            GetAdvicesByObjectID.ReturnsCollectionFromEntitySet<Advice>("Advice");
            


            //builder.EntitySet<TaskRef>("TaskRefs");
            //builder.EntitySet<TaskUsage>("TaskUsages");
            //builder.EntitySet<Text>("Texts");
            //builder.EntitySet<User>("Users");
            //builder.EntitySet<Wish>("Wishes");

            builder.EntitySet<Report>("Reports").EntityType.HasKey(x => x.Id);
            builder.EntitySet<ExternalReference>("ExternalReferences").EntityType.HasKey(x => x.Id);

            var reportCategoryTypes = builder.EntitySet<ReportCategoryType>(nameof(ReportCategoryTypesController).Replace("Controller", string.Empty));
            reportCategoryTypes.EntityType.HasKey(x => x.Id);

            var helpTexts = builder.EntitySet<HelpText>(nameof(HelpTextsController).Replace("Controller", string.Empty));
            helpTexts.EntityType.HasKey(x => x.Id);

            return builder.GetEdmModel();
        }
    }
}
