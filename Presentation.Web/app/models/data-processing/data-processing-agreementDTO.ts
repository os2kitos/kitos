module Kitos.Models.DataProcessing {
    export interface IDataProcessingAgreementDTO {
        id: number,
        name: string,
        organizationId: number,
        objectOwnerId: number,
        referenceId: number,
        references: Array<DpaReference>;
    }

    export interface DpaReference extends BaseReference {
        Title: string;
        ExternalReferenceId: string;
        URL: string;
        MasterReference: boolean;
        Created: Date;
    }
}