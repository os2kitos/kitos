module Kitos.Models.DataProcessing {
    export interface IDataProcessingRegistrationRight {
        UserId: number;
        RoleId: number;
        ObjectId: number;
        User: IUser;
        Role: IRoleEntity;
        Object: IDataProcessingRegistration;
    }
}
