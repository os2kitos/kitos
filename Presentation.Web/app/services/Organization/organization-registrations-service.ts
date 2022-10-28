module Kitos.Services.Organization {

    export interface IOrganizationRegistrationsService {
        getRegistrations(orgId: number): ng.IPromise<Models.Api.Organization.OrganizationRegistrationDetailsDto>;
        deleteSelectedRegistrations(orgId: number, body: Models.Api.Organization.OrganizationRegistrationChangeRequestDto): angular.IPromise<boolean>;
        deleteOrganizationUnit(unitId: number, organizationId: number): angular.IPromise<boolean>;
        transferSelectedRegistrations(orgId: number, targetUnitId: number, body: Models.Api.Organization.OrganizationRegistrationChangeRequestDto): angular.IPromise<void>;
    }

    export class OrganizationRegistrationsService implements IOrganizationRegistrationsService {

        getRegistrations(orgId: number): ng.IPromise<Models.Api.Organization.OrganizationRegistrationDetailsDto> {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(`api/v1/organization-registrations/${orgId}`)
                .then(
                    result => {
                        var response = result.data as { response: Models.Api.Organization.OrganizationRegistrationDetailsDto }
                        return response.response;
                    },
                    error => this.apiWrapper.handleServerError(error)
                );
        }

        deleteSelectedRegistrations(orgId: number, body: Models.Api.Organization.OrganizationRegistrationChangeRequestDto): angular.IPromise<boolean> {
            return this.apiUseCaseFactory
                .createDeletion("Registreringer",
                    () => this.apiWrapper.delete(`api/v1/organization-registrations/${orgId}`, body))
                .executeAsync();
        }

        deleteOrganizationUnit(unitId: number, organizationId: number): angular.IPromise<boolean> {
            return this.apiUseCaseFactory
                .createDeletion("Organisationsenhed",
                () => this.apiWrapper.delete(`api/v1/organization-registrations/unit/${unitId}/${organizationId}`))
                .executeAsync();
        }

        transferSelectedRegistrations(orgId: number, targetUnitId: number, body: Models.Api.Organization.OrganizationRegistrationChangeRequestDto): angular.IPromise<void> {
            return this.apiUseCaseFactory
                .createUpdate("Registreringer",
                    () => this.apiWrapper.put(`api/v1/organization-registrations/${orgId}/${targetUnitId}`, body))
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