module Kitos.Models.ItProject {
    export interface IItProjectRight {
        UserId: number;
        RoleId: number;
        ObjectId: number;
        User: IUser;
        Role: IRoleEntity;
        Object: IItProject;
    }
}
