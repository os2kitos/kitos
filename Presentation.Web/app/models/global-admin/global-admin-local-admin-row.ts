module Kitos.Models.GlobalAdmin {

    export interface ILocalAdminRow {
        id: number
        organization: string
        name: string
        email: string
        objectContext: Models.Api.Organization.IAdminRightsDto
    }
}