module Kitos.Services.Organization {

    export interface IOrganizationRegistrationsService {
        getRegistrations(orgId: string, unitId: string): ng.IPromise<Models.Api.Organization.OrganizationRegistrationDetailsDto>;
        deleteSelectedRegistrations(orgId: string, unitId: string, body: Models.Api.Organization.OrganizationRegistrationChangeRequestDto): angular.IPromise<boolean>;
        deleteOrganizationUnit(organizationId: string, unitId: string): angular.IPromise<boolean>;
        transferSelectedRegistrations(orgId: string, unitId: string, targetUnitId: string, body: Models.Api.Organization.OrganizationRegistrationChangeRequestDto): angular.IPromise<void>;
    }

    export class OrganizationRegistrationsService implements IOrganizationRegistrationsService {

        getRegistrations(orgUuid: string, unitUuid: string): ng.IPromise<Models.Api.Organization.OrganizationRegistrationDetailsDto> {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(`api/v1/organizations/${orgUuid}/organization-units/${unitUuid}/registrations`)
                .then(
                    result => {
                        var response = result.data as { response: Models.Api.Organization.OrganizationRegistrationDetailsDto }
                        return response.response;
                    },
                    error => this.apiWrapper.handleServerError(error)
                );
        }

        deleteSelectedRegistrations(orgUuid: string, unitUuid: string, body: Models.Api.Organization.OrganizationRegistrationChangeRequestDto): angular.IPromise<boolean> {
            return this.apiUseCaseFactory
                .createDeletion("Registreringer",
                    () => this.apiWrapper.delete(`api/v1/organizations/${orgUuid}/organization-units/${unitUuid}/registrations`, body))
                .executeAsync();
        }

        deleteOrganizationUnit(orgUuid: string, unitUuid: string): angular.IPromise<boolean> {
            return this.apiUseCaseFactory
                .createDeletion("Organisationsenhed",
                    () => this.apiWrapper.delete(`api/v1/organizations/${orgUuid}/organization-units/${unitUuid}`))
                .executeAsync();
        }

        transferSelectedRegistrations(orgUuid: string, unitUuid: string, targetUnitUuid: string, body: Models.Api.Organization.OrganizationRegistrationChangeRequestDto): angular.IPromise<void> {
            return this.apiUseCaseFactory
                .createUpdate("Registreringer",
                    () => this.apiWrapper.put(`api/v1/organizations/${orgUuid}/organization-units/${unitUuid}/registrations/${targetUnitUuid}`, body))
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