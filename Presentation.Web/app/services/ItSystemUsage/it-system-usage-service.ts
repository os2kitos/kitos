module Kitos.Services.ItSystemUsage {

    export interface IItSystemUsageService {
        getItSystemUsage(systemUsageId: number): ng.IPromise<any>;
        addDataLevel(systemUsageId: number, dataLevel: number);
        removeDataLevel(systemUsageId: number, dataLevel: number);
        patchSystemUsage(systemUsageId: number, orgId: number, payload: any);
        patchPersonalData(systemUsageId: number, personalDataValue: Models.ViewModel.ItSystemUsage.PersonalDataOption): ng.IPromise<void>;
        removePersonalData(systemUsageId: number, personalDataValue: Models.ViewModel.ItSystemUsage.PersonalDataOption): ng.IPromise<boolean>;
        //Odata kept here to keep all pages working as they used to
        patchSystem(id: number, payload: any);
        getValidationDetails(usageId: number): ng.IPromise<Models.ItSystemUsage.IItSystemUsageValidationDetailsResponseDTO>;
    }

    export class ItSystemUsageService implements IItSystemUsageService {
        static $inject = ["$http", "notify", "apiUseCaseFactory"];
        
        private readonly apiWrapper: Services.Generic.ApiWrapper;
        constructor(private readonly $http: ng.IHttpService,
            private readonly notify,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory) {
            this.apiWrapper = new Services.Generic.ApiWrapper($http);
        }

        private getBaseUrl(systemUsageId: number): string {
            return `api/v1/itsystemusage/${systemUsageId}`;
        }
        private getSensitivityLevelUrl(systemUsageId: number): string {
            return this.getBaseUrl(systemUsageId) + "/sensitivityLevel";
        }
        private getPersonalDataUrl(systemUsageId: number, personalDataValue: Models.ViewModel.ItSystemUsage.PersonalDataOption): string {
            return this.getSensitivityLevelUrl(systemUsageId) + `/personalData/${personalDataValue}`;
        }

        getValidationDetails(usageId: number): ng.IPromise<Models.ItSystemUsage.IItSystemUsageValidationDetailsResponseDTO> {
            return this.$http
                .get<API.Models.IApiWrapper<Models.ItSystemUsage.IItSystemUsageValidationDetailsResponseDTO>>(this.getBaseUrl(usageId) + `/validation-details`)
                .then(response => {
                    return response.data.response;
                });
        }
        
        addDataLevel(systemUsageId: number, dataLevel: number) {
            return this.$http.patch(this.getSensitivityLevelUrl(systemUsageId) + `/add`, dataLevel);
        }

        removeDataLevel(systemUsageId: number, dataLevel: number) {
            return this.$http.patch(this.getSensitivityLevelUrl(systemUsageId) + `/remove`, dataLevel);
        }
        
        patchPersonalData(systemUsageId: number, personalDataValue: Models.ViewModel.ItSystemUsage.PersonalDataOption): ng.IPromise<void> {
            return this.apiUseCaseFactory
                .createUpdate("Almindelige personoplysninger",
                    () => this.apiWrapper.patch(
                        this.getPersonalDataUrl(systemUsageId, personalDataValue)))
                .executeAsync();
        }

        removePersonalData(systemUsageId: number, personalDataValue: Models.ViewModel.ItSystemUsage.PersonalDataOption): ng.IPromise<boolean> {
            return this.apiUseCaseFactory
                .createUpdate("Almindelige personoplysninger",
                    () => this.apiWrapper.delete(
                        this.getPersonalDataUrl(systemUsageId, personalDataValue)))
                .executeAsync();
        }

        patchSystemUsage(systemUsageId: number, orgId: number, payload) {
            return this.$http.patch(`api/itsystemusage/${systemUsageId}?organizationId=${orgId}`, payload);
        }

        patchSystem = (id: number, payload: any) => {
            this.$http.patch(`/odata/ItSystemUsages(${id})`, payload)
                .then(() => {
                        this.notify.addSuccessMessage("Feltet er opdateret!");
                    },
                    () => this.notify.addErrorMessage("Fejl! Feltet kunne ikke opdateres!"));
        }
        getItSystemUsage(systemUsageId: number): ng.IPromise<any> {
            return this.$http.get<any>("api/itSystemUsage/" + systemUsageId)
                .then(result => result.data.response);
        }

    }

    app.service("itSystemUsageService", ItSystemUsageService);
}