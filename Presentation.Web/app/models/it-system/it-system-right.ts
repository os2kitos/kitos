module Kitos.Models.ItSystem {
    export interface IItSystemRight {
        UserId: number;
        RoleId: number;
        ObjectId: number;
        User: IUser;
        Role: IItSystemRole;
        Object: ItSystemUsage.IItSystemUsage;
    }
}
