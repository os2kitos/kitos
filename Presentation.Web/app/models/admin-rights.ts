module Kitos.Models {
    export interface IAdminRight extends IEntity {
        UserId: number;
        RoleId: number;
        ObjectId: number;
        User: IUser;
        Role: IAdminRole;
        Object: IOrganization;
        DefaultOrgUnitId: number;
        DefaultOrgUnit: IOrganizationUnit;
    }
}
