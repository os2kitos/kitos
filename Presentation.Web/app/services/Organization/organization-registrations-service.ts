module Kitos.Services.Organization {

    export interface IOrganizationRegistrationsService {
        getRegistrations(orgId: number): ng.IPromise<Models.Api.Organization.OrganizationRegistrationRootDto>;
        deleteSelectedRegistrations(orgId: number, body: Models.Api.Organization.OrganizationRegistrationDeleteRequest): angular.IPromise<boolean>;
    }

    export class OrganizationRegistrationsService implements IOrganizationRegistrationsService {

        getRegistrations(orgId: number): ng.IPromise<Models.Api.Organization.OrganizationRegistrationRootDto> {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(`api/v1/organization-registrations/${orgId}`)
                .then(
                    result => {
                        var response = result.data as { response: Models.Api.Organization.OrganizationRegistrationRootDto }
                        return response.response;
                    },
                    error => this.apiWrapper.handleServerError(error)
                );
        }

        deleteSelectedRegistrations(orgId: number, body: Models.Api.Organization.OrganizationRegistrationDeleteRequest): angular.IPromise<boolean> {
            return this.apiUseCaseFactory
                .createDeletion("Organization unit",
                    () => this.apiWrapper.delete(`api/v1/organization-registrations/${orgId}`, body))
                .executeAsync();
        }

        static $inject: string[] = ["$http", "apiUseCaseFactory"];

        private readonly apiWrapper: Services.Generic.ApiWrapper;
        constructor(private readonly $http: ng.IHttpService,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory) {
            this.apiWrapper = new Services.Generic.ApiWrapper($http);
        }
    }

    app.service("organizationRegistrationsService", OrganizationRegistrationsService);
}