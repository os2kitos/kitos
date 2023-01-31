module Kitos.Services.Organization {

    export enum AdminRightsType {
        LocalAdmin = 0
    }

    export interface IOrganizationRightService {
        remove(currentOrgId: number, roleOrgId: number, roleId: number, userId: number): ng.IPromise<boolean>;
        create(newOrgId: number, userToUpdateId: number, roleId: number): ng.IPromise<void>;
        getAllByRightsType(rightsType: AdminRightsType): ng.IPromise<Array<Models.Api.Organization.IAdminRightsDto>>;
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

        private getAdminRightsQueryByType(rightsType: AdminRightsType) {
            switch (rightsType) {
            case AdminRightsType.LocalAdmin:
                return "/?allLocalAdmins";
            default:
                throw "IOrganizationRightService.getAdminRightsQueryByType: Incorrect rightsType value";
            }
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

        getAllByRightsType(rightsType: AdminRightsType): ng.IPromise<Array<Models.Api.Organization.IAdminRightsDto>> {
            return this.apiWrapper.getDataFromUrl(`${this.getBaseUrl()}${this.getAdminRightsQueryByType(rightsType)}`);
        }
    }
    app.service("organizationRightService", OrganizationRightService);
}