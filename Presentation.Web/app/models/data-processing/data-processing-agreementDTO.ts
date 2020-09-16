module Kitos.Models.DataProcessing {

    export interface IAssignedRoleDTO {
        user: ISimpleUserDTO,
        role: IDataProcessingRoleDTO,
    }

    export interface IDataProcessingAgreementDTO {
        id: number,
        name: string,
        assignedRoles: IAssignedRoleDTO[],
    }

    export interface IDataProcessingRoleDTO {
        id: number,
        name: string,
        note: string,
        hasWriteAccess: boolean,
    }

    export interface ISimpleUserDTO {
        id: number,
        name: string,
        email: string,
    }
}