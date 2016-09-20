module Kitos.Models.Organization {
    export interface IOrganizationUnitRight {
        UserId: number;
        RoleId: number;
        ObjectId: number;
        User: IUser;
        Role: IOrganizationUnitRole;
        Object: IOrganizationUnit;
    }
}
