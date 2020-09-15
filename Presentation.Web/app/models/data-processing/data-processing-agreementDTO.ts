module Kitos.Models.DataProcessing {
    export interface IDataProcessingAgreementDTO {
        id: number,
        name: string,
        organizationId: number,
        reference: Array<BaseReference>;
    }
}