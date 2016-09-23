module Kitos.Models.ItContract {
	/** Associates a  with an it contract () in a specific . */
    export interface IItContractRight extends IEntity {
        UserId: number;
        RoleId: number;
        ObjectId: number;
        User: IUser;
        Role: IRoleEntity;
        Object: IItContract;
    }
}
