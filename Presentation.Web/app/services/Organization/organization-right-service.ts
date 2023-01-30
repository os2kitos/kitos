module Kitos.Services.Organization {

    export interface IOrganizationRightService {
        remove(currentOrgId: number, roleOrgId: number, roleId: number, userId: number): ng.IPromise<boolean>;
        create(newOrgId: number, userToUpdateId: number, roleId: number): ng.IPromise<void>;
        getAll(): ng.IPromise<Array<Models.Api.Organization.ILocalAdminRightsDto>>;
    }

    export class OrganizationRightService implements IOrganizationRightService{
        static $inject = ["$http", "apiUseCaseFactory"];

        private readonly apiWrapper: Services.Generic.ApiWrapper;
        constructor(private readonly $http: ng.IHttpService,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory) {
            this.apiWrapper = new Services.Generic.ApiWrapper($http);
        }

        private getBaseUrl(): string {
            return `api/OrganizationRight`;
        }

        private getBaseUrlWithIds(currentOrgId: number, roleOrgId: number, roleId: number, userId: number): string {
            return `${this.getBaseUrl()}/${roleOrgId}?rId=${roleId}&uId=${userId}&organizationId=${currentOrgId}`;
        }

        remove(currentOrgId: number, roleOrgId: number, roleId: number, userId: number): ng.IPromise<boolean> {
            return this.apiUseCaseFactory.createDeletion("Lokale administratorer",
                () => this.apiWrapper.delete(this.getBaseUrlWithIds(currentOrgId, roleOrgId, roleId, userId)))
                .executeAsync();
        }

        create(newOrgId: number, userToUpdateId: number, roleId: number): ng.IPromise<void> {

            const data = {
                userId: userToUpdateId,
                role: roleId,
                organizationId: newOrgId
            };
            return this.apiUseCaseFactory.createCreation("Lokale administratorer",
                () => this.apiWrapper.post<void>(`${this.getBaseUrl()}/${newOrgId}`, data))
                .executeAsync();
        }

        getAll(): ng.IPromise<Array<Models.Api.Organization.ILocalAdminRightsDto>> {
            return this.apiWrapper.getDataFromUrl(this.getBaseUrl());
        }
    }
    app.service("organizationRightService", OrganizationRightService);
}