module Kitos.Models.Api.Authorization {

    export interface EntityAccessRightsDTO {
        id: number;
        canView: boolean;
        canDelete: boolean;
        canEdit: boolean;
    }
}