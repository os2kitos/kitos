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
}