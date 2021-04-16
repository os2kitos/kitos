module Kitos.Models.ItSystemUsage {

    export interface IAssignedSystemUsageRole {
        RoleId: number;
        UserFullName: string;
    }

    export interface IItSystemUsageOverviewReadModel {
        Id: number;
        SourceEntityId: number;
        Name: string;
        ItSystemDisabled: boolean;
        IsActive: boolean;
        ItSystemUuid: string;
        LocalSystemId: string | null;
        Version: string | null;
        LocalCallName: string | null;
        ParentItSystemId: number;
        ParentItSystemName: string | null;
        RoleAssignments: IAssignedSystemUsageRole[];
        ResponsibleOrganizationUnitId: number | null;
        ResponsibleOrganizationUnitName: string | null;
        ItSystemRightsHolderName: string | null;
        ItSystemBusinessTypeId: number | null;
        ItSystemBusinessTypeName: string | null;
    }
}