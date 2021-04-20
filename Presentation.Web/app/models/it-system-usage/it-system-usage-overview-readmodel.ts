module Kitos.Models.ItSystemUsage {

    export interface IAssignedSystemUsageRole {
        RoleId: number;
        UserFullName: string;
    }

    export interface IItSystemUsageOverviewTaskRefReadModel {
        Id: number;
        KLEId: string;
        KLEName: string;
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
        ItSystemTaskRefs: IItSystemUsageOverviewTaskRefReadModel[];
        LocalReferenceDocumentId: string | null;
        LocalReferenceUrl: string | null;
        LocalReferenceTitle: string | null;
        ObjectOwnerName: string | null;
        LastChangedByName: string | null;
        LastChanged: Date | null;
        Concluded: Date | null;
        MainContractSupplierName: string | null;
        SensitiveDataLevelsAsCsv : string | null;
    }
}