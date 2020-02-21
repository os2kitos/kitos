module Kitos.Models.Api.Authorization {

    export interface EntityAccessRightsDTO {
        canView: boolean;
        canDelete: boolean;
        canEdit: boolean;
    }
}