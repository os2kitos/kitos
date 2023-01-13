module Kitos.Services.Organization {
    export interface IOrganizationRightService {
        removeRight(currentOrgId: number, roleOrgId: number, roleId: number, userId: number): ng.IPromise<boolean>;
    }

    export class OrganizationRightService implements IOrganizationRightService{
        static $inject = ["$http", "apiUseCaseFactory"];

        private readonly apiWrapper: Services.Generic.ApiWrapper;
        constructor(private readonly $http: ng.IHttpService,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory) {
            this.apiWrapper = new Services.Generic.ApiWrapper($http);
        }

        private getBaseUrl(currentOrgId: number, roleOrgId: number, roleId: number, userId: number): string {
            return `api/OrganizationRight/${roleOrgId}?rId=${roleId}&uId=${userId}&organizationId=${currentOrgId}`;
        }

        removeRight(currentOrgId: number, roleOrgId: number, roleId: number, userId: number): ng.IPromise<boolean> {
            return this.apiUseCaseFactory.createDeletion("Lokale administratorer",
                    () => this.apiWrapper.delete(this.getBaseUrl(currentOrgId, roleOrgId, roleId, userId)))
                .executeAsync();
        }
    }
    app.service("organizationRightService", OrganizationRightService);
}