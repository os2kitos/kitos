module Kitos.Models.Generic.NamedEntity {

    export interface NamedEntityDTO {
        id: number;
        name: string;
    }

    export interface NamedEntityWithEnabledStatusDTO extends NamedEntityDTO{
        disabled : boolean;
    }

    export interface NamedEntityWithDescriptionAndExpirationStatusDTO extends NamedEntityDTO {
        expired: boolean;
        description: string;
    }
    export interface NamedEntityWithUserFullNameDTO extends NamedEntityDTO {
        userFullName: string;
    }

    export interface EntityWithOrganizationRelationshipDTO extends NamedEntityDTO {
        organization : Organization.ShallowOrganizationDTO
    }
}