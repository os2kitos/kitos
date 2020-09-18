module Kitos.Models.DataProcessing {
    export interface IDataProcessingAgreementDTO {
        id: number,
        name: string,
        organizationId: number,
        objectOwnerId: number,
        referenceId: number,
        references: Array<DpaReference>;
		itSystems: Models.Generic.NamedEntity.NamedEntityWithEnabledStatusDTO[];
    }

    export interface DpaReference extends BaseReference {
        Title: string;
        ExternalReferenceId: string;
        URL: string;
        MasterReference: boolean;
        Created: Date;
    }
}