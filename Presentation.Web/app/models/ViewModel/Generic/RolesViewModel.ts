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

    export interface IEditableAssignedRoleViewModel {
        user: IUserViewModel,
        role: IRoleViewModel,
        newUser: Select2OptionViewModel,
        newRoleIdAsString: string,
        isEditing?: boolean,
        editUserOptions: (input: number) => any,
    }

}