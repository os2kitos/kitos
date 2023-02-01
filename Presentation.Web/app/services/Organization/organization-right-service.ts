module Kitos.Services.Organization {
    
    export interface IOrganizationRightService {
        remove(orgId: number, roleId: number, userId: number): ng.IPromise<boolean>;
        create(newOrgId: number, userToUpdateId: number, roleId: number): ng.IPromise<void>;
        getAllByRightsType(rightsType: API.Models.OrganizationRole.LocalAdmin): ng.IPromise<Array<Models.Api.Organization.IAdminRightsDto>>;
    }

    export class OrganizationRightService implements IOrganizationRightService{
        static $inject = ["genericApiWrapper", "apiUseCaseFactory"];

        constructor(private readonly apiWrapper: Services.Generic.ApiWrapper,
            private readonly apiUseCaseFactory: Services.Generic.IApiUseCaseFactory) {
        }

        private getBaseUrl(): string {
            return `api/OrganizationRight`;
        }

        private getBaseUrlWithIds(orgId: number, roleId: number, userId: number): string {
            return `${this.getBaseUrl()}/${orgId}?rId=${roleId}&uId=${userId}&organizationId=${orgId}`;
        }

        private getAdminRightsQueryByType(rightsType: API.Models.OrganizationRole) {
            switch (rightsType) {
                case API.Models.OrganizationRole.LocalAdmin:
                    return "/?allLocalAdmins";
                default:
                    throw "IOrganizationRightService.getAdminRightsQueryByType: Incorrect rightsType value";
            }
        }

        remove(orgId: number, roleId: number, userId: number): ng.IPromise<boolean> {
            return this.apiUseCaseFactory.createDeletion("Lokal Administrator",
                () => this.apiWrapper.delete(this.getBaseUrlWithIds(orgId, roleId, userId)))
                .executeAsync();
        }

        create(newOrgId: number, userToUpdateId: number, roleId: number): ng.IPromise<void> {

            const data = {
                userId: userToUpdateId,
                role: roleId,
                organizationId: newOrgId
            };
            return this.apiUseCaseFactory.createCreation("Lokal Administrator",
                () => this.apiWrapper.post<void>(`${this.getBaseUrl()}/${newOrgId}`, data))
                .executeAsync();
        }

        getAllByRightsType(rightsType: API.Models.OrganizationRole.LocalAdmin): ng.IPromise<Array<Models.Api.Organization.IAdminRightsDto>> {
            return this.apiWrapper.getDataFromUrl(`${this.getBaseUrl()}${this.getAdminRightsQueryByType(rightsType)}`);
        }
    }
    app.service("organizationRightService", OrganizationRightService);
}