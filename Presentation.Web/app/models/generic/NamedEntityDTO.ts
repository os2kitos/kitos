module Kitos.Models.Generic.NamedEntity {

    export interface NamedEntityDTO {
        id: number;
        name: string;
    }

    export interface NamedEntityWithEnabledStatusDTO {
        id: number;
        name: string;
        disabled : boolean;
    }
}