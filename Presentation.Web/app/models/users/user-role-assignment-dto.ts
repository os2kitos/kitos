module Kitos.Models.Users {

    export enum BusinessRoleScope {
        ItSystemUsage,
        ItContract,
        DataProcessingRegistration,
        ItProject
    }

    export interface IAssignedRightDTO {
        rightId: number;
        businessObjectName: string;
        bBusinessRoleScope: BusinessRoleScope;
    }

    export interface UserRoleAssigmentDTO {
        administrativeAccessRoles: Array<Models.OrganizationRole>;
        rights: Array<IAssignedRightDTO>;
    }
}