module Kitos.Models.Api.Organization {
    export interface Organization extends Models.Generic.NamedEntity.NamedEntityDTO{
        uuid: string;
        canCvrBeModified: boolean;
    }
}