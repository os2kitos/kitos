module Kitos.Models.ItContract {

    export interface IItContractDTO {
        id: number,
        name: string,
        dataProcessingRegistrations: Models.Generic.NamedEntity.NamedEntityDTO[];
    }

    export interface IItContractOptions {
        criticalityOptions: Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO[]
    }
}