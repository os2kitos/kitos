module Kitos.Models {
    export interface IPasswordResetRequest extends IEntity {
        Hash: string;
        /** Time of the issue */
        Time: Date;
        UserId: number;
        User: IUser;
    }
}
