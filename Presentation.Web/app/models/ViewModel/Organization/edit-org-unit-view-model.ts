module Kitos.Models.ViewModel.Organization {

    export interface IEditOrgUnitViewModel {
        id: number,
        oldName: string,
        newName: string,
        newEan: string,
        newParent: number,
        localId: number,
        orgId: number,
        isRoot: boolean
    }
}