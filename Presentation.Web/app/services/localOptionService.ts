module Kitos.Services.LocalOptions {
    "use strict";
    import Services = Kitos.Services;

    export interface ILocalOptionService {
        getAll(): ng.IPromise<Models.IOptionEntity[]>
    }

    class LocalOptionService implements ILocalOptionService {
        constructor(
            private readonly $http: ng.IHttpService,
            private readonly userService: IUserService,
        private readonly routePrefix: string) {
        }

        private getBasePath() {
            return `odata/${this.routePrefix}`;
        }

        getAll(): ng.IPromise<Models.IOptionEntity[]> {
            return this
                .userService
                .getUser()
                .then(user => this.$http.get(
                    `${this.getBasePath()
                    }?$filter=IsLocallyAvailable eq true or IsObligatory&$orderby=Priority desc&organizationId=${user.currentOrganizationId}`))
                .then(result => result.data.value as Models.IOptionEntity[]);
        }
    }

    export enum LocalOptionType {
        ItSystemRoles,
        ItContractRoles,
        ItProjectRoles,
        AgreementElementTypes,
        ArchiveLocations,
        ArchiveTestLocations,
        ArchiveTypes,
        BusinessTypes,
        DataTypes,
        FrequencyTypes,
        GoalTypes,
        HandoverTrialTypes,
        InterfaceTypes,
        ItContractTemplateTypes,
        ItContractTypes,
        ItProjectTypes,
        ItSystemCategories,
        OptionExtendTypes,
        OrganizationUnitRoles,
        PaymentFrequencyTypes,
        PaymentModelTypes,
        PriceRegulationTypes,
        ProcurementStrategyTypes,
        PurchaseFormTypes,
        RegisterTypes,
        ReportCategoryTypes,
        SensistivePersonalDataTypes
    }

    export interface ILocalOptionServiceFactory {
        create(type: LocalOptionType): ILocalOptionService;
    }

    export class LocalOptionServiceFactory implements ILocalOptionServiceFactory{
        static $inject = ["$http","userService"];
        constructor(
            private readonly $http: ng.IHttpService,
            private readonly userService: IUserService) {

        }

        private getPrefix(type: LocalOptionType) : string {
            switch (type) {
                case LocalOptionType.ItSystemRoles:
                    return "LocalItSystemRoles";
                case LocalOptionType.ItContractRoles:
                    return "LocalItContractRoles";
                case LocalOptionType.ItProjectRoles:
                    return "LocalItProjectRoles";
                case LocalOptionType.AgreementElementTypes:
                    return "LocalAgreementElementTypes";
                case LocalOptionType.ArchiveLocations:
                    return "LocalArchiveLocations";
                case LocalOptionType.ArchiveTestLocations:
                    return "LocalArchiveTestLocations";
                case LocalOptionType.ArchiveTypes:
                    return "LocalArchiveTypes";
                case LocalOptionType.BusinessTypes:
                    return "LocalBusinessTypes";
                case LocalOptionType.DataTypes:
                    return "LocalDataTypes";
                case LocalOptionType.FrequencyTypes:
                    return "LocalFrequencyTypes";
                case LocalOptionType.GoalTypes:
                    return "LocalGoalTypes";
                case LocalOptionType.HandoverTrialTypes:
                    return "LocalHandoverTrialTypes";
                case LocalOptionType.InterfaceTypes:
                    return "LocalInterfaceTypes";
                case LocalOptionType.ItContractTemplateTypes:
                    return "LocalItContractTemplateTypes";
                case LocalOptionType.ItContractTypes:
                    return "LocalItContractTypes";
                case LocalOptionType.ItProjectTypes:
                    return "LocalItProjectTypes";
                case LocalOptionType.ItSystemCategories:
                    return "LocalItSystemCategories";
                case LocalOptionType.OptionExtendTypes:
                    return "LocalOptionExtendTypes";
                case LocalOptionType.OrganizationUnitRoles:
                    return "LocalOrganizationUnitRoles";
                case LocalOptionType.PaymentFrequencyTypes:
                    return "LocalPaymentFrequencyTypes";
                case LocalOptionType.PaymentModelTypes:
                    return "LocalPaymentModelTypes";
                case LocalOptionType.PriceRegulationTypes:
                    return "LocalPriceRegulationTypes";
                case LocalOptionType.ProcurementStrategyTypes:
                    return "LocalProcurementStrategyTypes";
                case LocalOptionType.PurchaseFormTypes:
                    return "LocalPurchaseFormTypes";
                case LocalOptionType.RegisterTypes:
                    return "LocalRegisterTypes";
                case LocalOptionType.ReportCategoryTypes:
                    return "LocalReportCategoryTypes";
                case LocalOptionType.SensistivePersonalDataTypes:
                    return "LocalSensistivePersonalDataTypes";
            default:
                throw new Error(`Unknown option type ${type}`);
            }
        }

        create(type: LocalOptionType): ILocalOptionService {
            return new LocalOptionService(this.$http, this.userService, this.getPrefix(type));
        }
    }

    app.service("localOptionServiceFactory", LocalOptionServiceFactory);
}