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

    export interface IUserWithApiAccessModifier extends IUserWithEmail {
        apiAccess: boolean
    }

    export interface IUserWithCrossAccess extends IUserWithApiAccessModifier {
        stakeholderAccess: boolean,
        organizationsWithRights: string[]
    }

    export interface IUserWithOrganizationName extends IUserWithApiAccessModifier {
        orgName: string
    }
}