module Kitos.Models.ViewModel.Generic.Roles {

    export interface IRoleViewModel {
        id: number,
        name: string,
        note: string,
        hasWriteAccess: boolean,
    }

    export interface IUserViewModel {
        id: number,
        name: string,
        email: string,
    }

    export interface IUserOptionsViewModel {
        id: number,
        text: string,
        user: IUserViewModel,
    }

    export interface IRoleOptionsViewModel {
        id: number,
        text: string,
        role: IRoleViewModel,
    }

    export interface IEditableAssignedRoleViewModel {
        user: IUserViewModel,
        role: IRoleViewModel,
        newUser: IUserOptionsViewModel,
        newRoleIdAsString: string,
    }

}