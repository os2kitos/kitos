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

    export interface IUserWithEmail extends Models.Generic.NamedEntity.NamedEntityDTO {
        email: string,
    }

    export interface IUserWithCrossAccess extends IUserWithEmail {
        apiAccess: boolean,
        stakeholderAccess: boolean
    }

    export interface IUserWithOrganizationName extends IUserWithEmail {
        orgName: string
    }
}