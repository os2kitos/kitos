module Kitos.Models.Generic.NamedEntity {

    export interface NamedEntityDTO {
        id: number;
        name: string;
    }

    export interface NamedEntityWithEnabledStatusDTO extends NamedEntityDTO{
        disabled : boolean;
    }

    export interface NamedEntityWithExpirationStatusDTO extends NamedEntityDTO {
        expired: boolean;
    }

    export interface NamedEntityWithDescriptionAndExpirationStatusDTO extends NamedEntityWithExpirationStatusDTO {
        description: string;
    }

    export interface NamedEntityWithUserFullNameDTO extends NamedEntityDTO {
        userFullName: string;
    }

    export interface EntityWithOrganizationRelationshipDTO extends NamedEntityDTO {
        organization : Organization.ShallowOrganizationDTO
    }
}