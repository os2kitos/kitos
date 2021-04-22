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

    export interface IItSystemUsageOverviewDataProcessingRegistrationReadModel {
        DataProcessingRegistrationId: number;
        DataProcessingRegistrationName: string;
        IsAgreementConcluded: string | null;
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
        ParentItSystemDisabled : boolean | null;
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
        MainContractIsActive: boolean | null;
        HasMainContract: boolean;
        SensitiveDataLevelsAsCsv: string | null;
        ItProjectNamesAsCsv: string | null;
        ArchiveDuty: string | null;
        IsHoldingDocument: boolean;
        ActiveArchivePeriodEndDate: Date | null;
        RiskSupervisionDocumentationName: string | null;
        RiskSupervisionDocumentationUrl: string | null;
        LinkToDirectoryName: string | null;
        LinkToDirectoryUrl: string | null;
        DataProcessingRegistrationsConcludedAsCsv: string | null;
        DataProcessingRegistrationNamesAsCsv: string | null;
        DataProcessingRegistrations: IItSystemUsageOverviewDataProcessingRegistrationReadModel[];
        GeneralPurpose: string | null;
        HostedAt: string | null;
    }
}