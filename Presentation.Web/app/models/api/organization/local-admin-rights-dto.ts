module Kitos.Models.Api.Organization {

    export interface ILocalAdminRightsDto {
        userName: string,
        fullName: string,
        userEmail: string,
        organizationName: string,
        organizationId: number,
        role: number,
        userId: number,
    }
}