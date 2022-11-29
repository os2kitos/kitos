module Kitos.Models.ViewModel.Organization {

    export interface IEditOrgUnitViewModel {
        id: number,
        uuid: string,
        oldName: string,
        newName: string,
        newEan: string,
        oldParent: number,
        newParent: number,
        oldLocalId: number,
        localId: number,
        orgId: number,
        isRoot: boolean,
        isFkOrganizationUnit: boolean,
    }

    export enum OrganizationUnitEditResultType {
        RightsChanged = 1,
        FieldsChanged = 2,
        UnitRelocated = 3,
        UnitDeleted = 4,
        SubUnitCreated = 5
    }

    export interface IOrganizationUnitEditResult{
        types: OrganizationUnitEditResultType[],
        scopeToUnit: any,
    }
}