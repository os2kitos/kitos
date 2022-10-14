module Kitos.Services.Organization {

    export interface IOrganizationRegistrationsService {
        getRegistrations(orgId: number): ng.IPromise<Models.Api.Organization.OrganizationRegistrationRootDto>;
    }

    export class OrganizationRegistrationsService implements IOrganizationRegistrationsService {

        getRegistrations(orgId: number): ng.IPromise<Models.Api.Organization.OrganizationRegistrationRootDto> {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(`unit-registrations/${orgId}`)
                .then(
                    result => {
                        var response = result.data as { response: Models.Api.Organization.OrganizationRegistrationRootDto }
                        return response.response;
                    },
                    error => this.apiWrapper.handleServerError(error)
                );
        }

        static $inject: string[] = ["$http"];

        private readonly apiWrapper: Services.Generic.ApiWrapper;
        constructor(private readonly $http: ng.IHttpService) {
            this.apiWrapper = new Services.Generic.ApiWrapper($http);
        }
    }
}