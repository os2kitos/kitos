module Kitos.Services.LocalOptions {
    "use strict";
    import Services = Kitos.Services;

    export interface ILocalOptionService {
        getAll(): ng.IPromise<Models.IOptionEntity[]>,
        get(id: number): ng.IPromise<Models.IOptionEntity>,
        update(id: number, entity: Models.IEditedLocalOptionEntity): ng.IPromise<boolean>,
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


        get(id: number): angular.IPromise<Models.IOptionEntity> {
            return this
                .userService
                .getUser()
                .then(user => this.$http.get(`${this.getBasePath()}(${id})?organizationId=${user.currentOrganizationId}`))
                .then(result => result.data as Models.IOptionEntity);
        }

        update(id: number, entity: Models.IEditedLocalOptionEntity): angular.IPromise<boolean> {
            return this
                .userService
                .getUser()
                .then(user => this.$http.patch(
                    `${this.getBasePath()}(${id})?organizationId=${user.currentOrganizationId}`, entity))
                .then(result => {
                    if (result.status === 200) {
                        return true;
                    } else {
                        return false;
                    }
                });
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
        SensistivePersonalDataTypes,
        SensitiveDataTypes,
        TerminationDeadlineTypes,
        DataProcessingAgreementRoles,
        DataProcessingBasisForTransferOptions,
        DataProcessingOversightOptions,
        DataProcessingDataResponsibleOptions,
        DataProcessingCountryOptions,
    }

    export interface ILocalOptionUrlResolver {
        resolveKendoGridGetUrl(type: LocalOptionType, orgId: number): string;
        resolveAutosaveUrl(type: LocalOptionType): string;
    }

    export class LocalOptionUrlResolver implements ILocalOptionUrlResolver {
        static $inject = ["localOptionTypeMapper"];
        constructor(
            private readonly localOptionTypeMapper: ILocalOptionTypeMapper) {
        }

        resolveKendoGridGetUrl(type: LocalOptionType, orgId: number): string {
            return `/odata/${this.localOptionTypeMapper.getOdataController(type)}?organizationId=${orgId}`;
        }

        resolveAutosaveUrl(type: LocalOptionType): string {
            return `/odata/${this.localOptionTypeMapper.getOdataController(type)}`;
        }
    }
    app.service("localOptionUrlResolver", LocalOptionUrlResolver);

    export interface ILocalOptionTypeMapper {
        getOdataController(type: LocalOptionType): string;
    }

    export class LocalOptionTypeMapper implements ILocalOptionTypeMapper {
        getOdataController(type: LocalOptionType) {
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
                case LocalOptionType.SensitiveDataTypes:
                    return "LocalSensitiveDataTypes";
                case LocalOptionType.TerminationDeadlineTypes:
                    return "LocalTerminationDeadlineTypes";
                case LocalOptionType.DataProcessingAgreementRoles:
                    return "LocalDataProcessingRegistrationRoles";
                case LocalOptionType.DataProcessingBasisForTransferOptions:
                    return "LocalDataProcessingBasisForTransferOptions";
                case LocalOptionType.DataProcessingOversightOptions:
                    return "LocalDataProcessingOversightOptions";
                case LocalOptionType.DataProcessingDataResponsibleOptions:
                    return "LocalDataProcessingDataResponsibleOptions";
                case LocalOptionType.DataProcessingCountryOptions:
                    return "LocalDataProcessingCountryOptions";
                default:
                    throw new Error(`Unknown option type ${type}`);
            }
        }
    }
    app.service("localOptionTypeMapper", LocalOptionTypeMapper);

    export interface ILocalOptionServiceFactory {
        create(type: LocalOptionType): ILocalOptionService;
    }

    export class LocalOptionServiceFactory implements ILocalOptionServiceFactory{
        static $inject = ["$http", "userService", "localOptionTypeMapper"];
        constructor(
            private readonly $http: ng.IHttpService,
            private readonly userService: IUserService,
            private readonly localOptionTypeMapper: ILocalOptionTypeMapper) {
        }

        create(type: LocalOptionType): ILocalOptionService {
            return new LocalOptionService(this.$http, this.userService, this.localOptionTypeMapper.getOdataController(type));
        }
    }

    app.service("localOptionServiceFactory", LocalOptionServiceFactory);
}