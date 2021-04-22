module Kitos.Models.Api {
    export interface IUser extends Models.Generic.NamedEntity.NamedEntityDTO {
        lastName: string,
        email: string,
        phoneNumber: string,
        defaultUserStartPreference: string,
        defaultOrganizationUnitId: number,
        isUsingDefaultOrgUnit: boolean,
        currentOrganizationName: string,
        currentOrganizationUnitName: string
    }
}