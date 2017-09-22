module Kitos.Models.ItSystem {
    export interface IItSystemRight {
        UserId: number;
        RoleId: number;
        ObjectId: number;
        User: IUser;
        Role: IRoleEntity;
        Object: ItSystemUsage.IItSystemUsage;
    }
}
