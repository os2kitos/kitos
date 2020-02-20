module Kitos.Models.Generic.Authorization {

    export interface EntityAccessRightsDTO {
        canView: boolean;
        canDelete: boolean;
        canEdit: boolean;
    }
}