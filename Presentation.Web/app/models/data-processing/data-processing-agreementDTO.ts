module Kitos.Models.DataProcessing {
    export interface IDataProcessingAgreementDTO {
        id: number,
        name: string,
        itSystems: Models.Generic.NamedEntity.NamedEntityWithEnabledStatusDTO[];
    }
}