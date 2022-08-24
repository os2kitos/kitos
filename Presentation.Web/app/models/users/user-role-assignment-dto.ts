module Kitos.Models.Users {

    export enum BusinessRoleScope {
        ItSystemUsage = 0,
        ItContract = 1,
        DataProcessingRegistration = 2,
        //NOTE: 3 used to be ItProject
        OrganizationUnit = 4
    }

    export interface IAssignedRightDTO {
        rightId: number;
        roleName: string;
        businessObjectName: string;
        scope: BusinessRoleScope;
    }

    export interface UserRoleAssigmentDTO {
        administrativeAccessRoles: Array<Models.OrganizationRole>;
        rights: Array<IAssignedRightDTO>;
    }
}