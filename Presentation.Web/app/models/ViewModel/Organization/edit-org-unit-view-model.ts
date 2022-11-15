module Kitos.Models.ViewModel.Organization {

    export interface IEditOrgUnitViewModel {
        id: number,
        oldName: string,
        newName: string,
        newEan: string,
        currentParent: number,
        newParent: number,
        localId: number,
        orgId: number,
        isRoot: boolean,
        isFkOrganizationUnit: boolean,
    }

    export enum OrganizationUnitEditResultType {
        None = 0,
        RightsChanged = 1,
        UnitRelocated = 2,
        UnitDeleted = 3,
        SubUnitCreated = 4
    }

    export interface IOrganizationUnitEditResult{
        type: OrganizationUnitEditResultType,
        scopeToUnit: any,
    }
}