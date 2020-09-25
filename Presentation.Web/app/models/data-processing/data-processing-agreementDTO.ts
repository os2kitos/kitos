module Kitos.Models.DataProcessing {

    export interface IAssignedRoleDTO {
        user: ISimpleUserDTO,
        role: IDataProcessingRoleDTO,
    }

    export interface IDataProcessingRegistrationDTO {
        id: number,
        name: string,
        references: Array<IDataProcessingReferenceDTO>;
        itSystems: Models.Generic.NamedEntity.NamedEntityWithEnabledStatusDTO[];
        assignedRoles: IAssignedRoleDTO[];
        dataProcessors: IDataProcessorDTO[];
        agreementConcluded: Models.Generic.ValueOptionWithOptionalDateDTO<Models.Api.Shared.YesNoIrrelevantOption>;
    }

    export interface IDataProcessingReferenceDTO extends BaseReference {
        title: string;
        externalReferenceId: string;
        url: string;
        masterReference: boolean;
        created: Date;
        itSystems: Models.Generic.NamedEntity.NamedEntityWithEnabledStatusDTO[];
        assignedRoles: IAssignedRoleDTO[],
    }

    export interface IDataProcessorDTO extends Models.Generic.NamedEntity.NamedEntityDTO {
        cvrNumber: string,
    }

    export interface IDataProcessingRoleDTO extends Models.Generic.NamedEntity.NamedEntityDTO {
        note: string,
        hasWriteAccess: boolean,
    }

    export interface ISimpleUserDTO extends Models.Generic.NamedEntity.NamedEntityDTO {
        email: string,
    }
}