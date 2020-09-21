module Kitos.Models.DataProcessing {
    export interface IDataProcessingAgreementDTO {
        id: number,
        name: string,
        organizationId: number,
        objectOwnerId: number,
        referenceId: number,
        references: Array<dpaReference>;
		itSystems: Models.Generic.NamedEntity.NamedEntityWithEnabledStatusDTO[];
    }

    export interface dpaReference extends BaseReference {
        title: string;
        externalReferenceId: string;
        url: string;
        masterReference: boolean;
        created: Date;
    }
}