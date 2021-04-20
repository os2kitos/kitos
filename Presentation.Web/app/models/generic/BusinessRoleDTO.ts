module Kitos.Models.Generic.Roles {

    export interface BusinessRoleDTO extends Generic.NamedEntity.NamedEntityDTO {
        note: string,
        hasWriteAccess: boolean,
        expired: boolean;
    }
}