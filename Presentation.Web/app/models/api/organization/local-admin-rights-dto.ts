module Kitos.Models.Api.Organization {

    export interface ILocalAdminRightsDto {
        userName: string,
        userEmail: string,
        organizationName: string,
        organizationId: number,
        role: number,
        userId: number,
        user: //TODO: add user property
    }
}