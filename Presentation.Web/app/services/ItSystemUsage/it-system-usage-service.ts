module Kitos.Services.ItSystemUsage {

    export interface IItSystemUsageService {
        addDataLevel(systemUsageId: number, dataLevel: number);
        removeDataLevel(systemUsageId: number, dataLevel: number);
        patchSystemUsage(systemUsageId: number, orgId: number, payload: any);
        //Odata kept here to keep all pages working as they used to
        patchSystem(id: number, payload: any);
        getValidationDetails(usageId: number): ng.IPromise<Models.ItSystemUsage.IItSystemUsageValidationDetailsResponseDTO>;
    }

    export class ItSystemUsageService implements IItSystemUsageService {
        getValidationDetails(usageId: number): ng.IPromise<Models.ItSystemUsage.IItSystemUsageValidationDetailsResponseDTO> {
            return this.$http
                .get<API.Models.IApiWrapper<Models.ItSystemUsage.IItSystemUsageValidationDetailsResponseDTO>>(`api/itsystemusage?usageId=${usageId}/validation-details`)
                .then(response => {
                    return response.data.response;
                });
        }

        static $inject = ["$http", "notify"];
        constructor(private readonly $http: ng.IHttpService, private notify) {
        }

        addDataLevel(systemUsageId: number, dataLevel: number) {
            return this.$http.patch(`api/v1/itsystemusage/${systemUsageId}/sensitivityLevel/add`, dataLevel);
        }

        removeDataLevel(systemUsageId: number, dataLevel: number) {
            return this.$http.patch(`api/v1/itsystemusage/${systemUsageId}/sensitivityLevel/remove`, dataLevel);
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
    }

    app.service("itSystemUsageService", ItSystemUsageService);
}