module Kitos.Services.Organization {

    export interface IOrganizationUnitService {
        getUnitAccessRights(unitId: number): ng.IPromise<Models.Api.Organization.OrganizationRegistrationDetailsDto>;
        getRegistrations(orgId: number): ng.IPromise<Models.Api.Organization.OrganizationRegistrationDetailsDto>;
        deleteSelectedRegistrations(orgId: number, body: Models.Api.Organization.OrganizationRegistrationChangeRequest): angular.IPromise<boolean>;
        deleteOrganizationUnit(unitId: number, organizationId: number): angular.IPromise<boolean>;
        transferSelectedRegistrations(orgId: number, targetUnitId: number, body: Models.Api.Organization.OrganizationRegistrationChangeRequest): angular.IPromise<void>;
    }

    export class OrganizationUnitService implements IOrganizationUnitService {
        getUnitAccessRights(unitId: number) {}

        getRegistrations(orgId: number): ng.IPromise<Models.Api.Organization.OrganizationRegistrationDetailsDto> {
            return this
                .$http
                .get<API.Models.IApiWrapper<any>>(`api/v1/organization-units/${orgId}`)
                .then(
                    result => {
                        var response = result.data as { response: Models.Api.Organization.OrganizationRegistrationDetailsDto }
                        return response.response;
                    },
                    error => this.apiWrapper.handleServerError(error)
                );
        }

        deleteSelectedRegistrations(orgId: number, body: Models.Api.Organization.OrganizationRegistrationChangeRequest): angular.IPromise<boolean> {
            return this.apiUseCaseFactory
                .createDeletion("Organization unit",
                    () => this.apiWrapper.delete(`api/v1/organization-units/${orgId}`, body))
                .executeAsync();
        }

        deleteOrganizationUnit(unitId: number, organizationId: number): angular.IPromise<boolean> {
            return this.apiWrapper.delete(`api/v1/organization-units/unit/${unitId}/${organizationId}`);
        }

        transferSelectedRegistrations(orgId: number, targetUnitId: number, body: Models.Api.Organization.OrganizationRegistrationChangeRequest): angular.IPromise<void> {
            return this.apiUseCaseFactory
                .createUpdate("Organization unit",
                    () => this.apiWrapper.put(`api/v1/organization-units/${orgId}/${targetUnitId}`, body))
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