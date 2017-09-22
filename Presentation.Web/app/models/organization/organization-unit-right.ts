module Kitos.Models {
    export interface IOrganizationUnitRight {
        UserId: number;
        RoleId: number;
        ObjectId: number;
        User: IUser;
        Role: IRoleEntity;
        Object: IOrganizationUnit;
    }
}
