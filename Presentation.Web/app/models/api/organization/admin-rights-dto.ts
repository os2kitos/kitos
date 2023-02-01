module Kitos.Models.Api.Organization {

    export interface IAdminRightsDto {
        userName: string
        fullName: string
        userEmail: string
        organizationName: string
        organizationId: number
        role: number
        userId: number
        user: IUser
    }
}