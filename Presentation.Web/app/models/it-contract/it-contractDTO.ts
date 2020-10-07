module Kitos.Models.ItContract {

    export interface IItContractDTO {
        id: number,
        name: string,
        dataProcessingRegistrations: Models.Generic.NamedEntity.NamedEntityDTO[];
    }
}