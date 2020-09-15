module Kitos.Models.DataProcessing {
    export interface IDataProcessingAgreementDTO {
        id: number,
        name: string,
        organizationId: number,
        objectOwnerId: number,
        referenceId: number,
        references: Array<BaseReference>;
    }
}