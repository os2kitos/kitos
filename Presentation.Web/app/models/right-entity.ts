module Kitos.Models {
    export interface IRightEntity<TRoot> {
        UserId: number;
        RoleId: number;
        ObjectId: number;
        User: IUser;
        Role: IRoleEntity;
        Object: TRoot;
    }
}