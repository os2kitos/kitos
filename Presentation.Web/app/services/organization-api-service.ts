module Kitos.Services {
    "use strict";

    export interface IOrganizationApiService {
        getOrganization(id: number): angular.IPromise<Models.Api.Organization.Organization>;
        getPermissions(uuid: string): angular.IPromise<Models.Api.Organization.OrganizationPermissionsDTO>;
        getOrganizationDeleteConflicts(uuid: string) : angular.IPromise<Models.Api.Organization.OrganizationDeleteConflicts>;
        getOrganizationUnit(organizationId: number): angular.IPromise<Models.Api.Organization.OrganizationUnit>;
        deleteOrganization(uuid: string, enforce : boolean): angular.IPromise<boolean>;
    }

    export class OrganizationApiService implements IOrganizationApiService {

        getOrganizationDeleteConflicts(uuid: string): angular.IPromise<Models.Api.Organization.OrganizationDeleteConflicts> {
            return this.apiWrapper.getDataFromUrl<Models.Api.Organization.OrganizationDeleteConflicts>(`api/v1/organizations/${uuid}/deletion/conflicts`);
        }

        getOrganization(id: number): angular.IPromise<Models.Api.Organization.Organization> {
            return this.apiWrapper.getDataFromUrl<Models.Api.Organization.Organization>(`api/organization/${id}`);
        }

        getOrganizationUnit(organizationId: number): angular.IPromise<Models.Api.Organization.OrganizationUnit> {
            return this.apiWrapper.getDataFromUrl<Models.Api.Organization.OrganizationUnit>(`api/organizationUnit?organization=${organizationId}`);
        }

        deleteOrganization(uuid: string, enforce : boolean): angular.IPromise<boolean> {
            return this.apiWrapper.delete(`api/v1/organizations/${uuid}/deletion?enforce=${enforce}`);
        }

        static $inject: string[] = ["$http"];
        private readonly apiWrapper: Services.Generic.ApiWrapper;
        constructor($http: ng.IHttpService) {
            this.apiWrapper = new Services.Generic.ApiWrapper($http); 
        }

        getPermissions(uuid: string): ng.IPromise<Models.Api.Organization.OrganizationPermissionsDTO> {
            return this.apiWrapper.getDataFromUrl<Models.Api.Organization.OrganizationPermissionsDTO>(`api/v1/organizations/${uuid}/permissions`);
        }
    }

    app.service("organizationApiService", OrganizationApiService);
}
