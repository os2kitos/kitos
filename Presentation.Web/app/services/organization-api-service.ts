module Kitos.Services {
    "use strict";

    export interface IOrganizationApiService {
        getOrganization(id: number): angular.IPromise<Models.Api.Organization.Organization>;
        getOrganizationDeleteConflicts(uuid: string) : angular.IPromise<Models.Api.Organization.OrganizationDeleteConflicts>;
        deleteOrganization(uuid: string, enforce : boolean): angular.IPromise<boolean>;
    }

    export class OrganizationApiService implements IOrganizationApiService {

        getOrganizationDeleteConflicts(uuid: string): angular.IPromise<Models.Api.Organization.OrganizationDeleteConflicts> {
            return this.apiWrapper.getDataFromUrl<Models.Api.Organization.OrganizationDeleteConflicts>(`api/v1/organizations/${uuid}/deletion/conflicts`);
        }

        getOrganization(id: number): angular.IPromise<Models.Api.Organization.Organization> {
            return this.apiWrapper.getDataFromUrl<Models.Api.Organization.Organization>(`api/organization/${id}`);
        }

        deleteOrganization(uuid: string, enforce : boolean): angular.IPromise<boolean> {
            return this.apiWrapper.delete(`api/v1/organizations/${uuid}/deletion?enforce=${enforce}`);
        }

        static $inject: string[] = ["$http"];
        private readonly apiWrapper: Services.Generic.ApiWrapper;
        constructor($http: ng.IHttpService) {
            this.apiWrapper = new Services.Generic.ApiWrapper($http);
        }
    }

    app.service("organizationApiService", OrganizationApiService);
}
