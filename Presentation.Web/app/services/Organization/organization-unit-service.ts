module Kitos.Services.Organization {

    export interface IOrganizationUnitService {
        getUnitAccessRights(orgUuid: string, unitUuid: string): ng.IPromise<Models.Api.Organization.UnitAccessRightsDto>;
        getRegistrations(orgUuid: string, unitUuid: string): ng.IPromise<Models.Api.Organization.OrganizationUnitRegistrationDetailsDto>;
        deleteSelectedRegistrations(orgUuid: string, unitUuid: string, body: Models.Api.Organization.OrganizationUnitRegistrationChangeRequestDto): angular.IPromise<boolean>;
        deleteOrganizationUnit(organizationId: string, unitUuid: string): angular.IPromise<boolean>;
        transferSelectedRegistrations(orgUuid: string, unitUuid: string, body: Models.Api.Organization.TransferOrganizationUnitRegistrationRequestDto): angular.IPromise<void>;
    }

    export class OrganizationUnitService implements IOrganizationUnitService {
        getUnitAccessRights(orgUuid: string, unitUuid: string): ng.IPromise<Models.Api.Organization.UnitAccessRightsDto> {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(`api/v1/organizations/${orgUuid}/organization-units/${unitUuid}/access-rights`)
                .then(
                    result => {
                        var response = result.data as { response: Models.Api.Organization.UnitAccessRightsDto }
                        return response.response;
                    },
                    error => this.apiWrapper.handleServerError(error)
                );
        }

        getRegistrations(orgUuid: string, unitUuid: string): ng.IPromise<Models.Api.Organization.OrganizationUnitRegistrationDetailsDto> {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(`api/v1/organizations/${orgUuid}/organization-units/${unitUuid}/registrations`)
                .then(
                    result => {
                        var response = result.data as { response: Models.Api.Organization.OrganizationUnitRegistrationDetailsDto }
                        return response.response;
                    },
                    error => this.apiWrapper.handleServerError(error)
                );
        }

        deleteSelectedRegistrations(orgUuid: string, unitUuid: string, body: Models.Api.Organization.OrganizationUnitRegistrationChangeRequestDto): angular.IPromise<boolean> {
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

        transferSelectedRegistrations(orgUuid: string, unitUuid: string, body: Models.Api.Organization.TransferOrganizationUnitRegistrationRequestDto): angular.IPromise<void> {
            return this.apiUseCaseFactory
                .createUpdate("Registreringer",
                    () => this.apiWrapper.put(`api/v1/organizations/${orgUuid}/organization-units/${unitUuid}/registrations`, body))
                .executeAsync();
        }

        static $inject: string[] = ["$http", "apiUseCaseFactory"];

        private readonly apiWrapper: Services.Generic.ApiWrapper;
        constructor(private readonly $http: ng.IHttpService,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory) {
            this.apiWrapper = new Services.Generic.ApiWrapper($http);
        }
    }

    app.service("organizationUnitService", OrganizationUnitService);
}