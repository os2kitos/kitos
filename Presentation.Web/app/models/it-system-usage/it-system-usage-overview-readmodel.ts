module Kitos.Models.ItSystemUsage {

    export interface IAssignedSystemUsageRole {
        RoleId: number;
        UserFullName: string;
        Email : string;
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

    export interface IItSystemUsageOverviewInterfaceReadModel {
        InterfaceId: number;
        InterfaceName: string;
    }

    export interface IItSystemUsageOverviewItSystemUsageReadModel {
        ItSystemUsageId: number;
        ItSystemUsageName: string;
    }

    export interface IItSystemUsageOverviewRelevantOrgUnitReadModel {
        OrganizationUnitName: string;
        OrganizationUnitId: number;
    }

    export interface IItSystemUsageOverviewReadModel {
        Id: number;
        SourceEntityId: number;
        SystemName: string;
        ItSystemDisabled: boolean;
        ActiveAccordingToValidityPeriod: boolean;
        ActiveAccordingToLifeCycle: boolean;
        Note: string;
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
        LastChangedAt: Date | null;
        Concluded: Date | null;
        ExpirationDate: Date | null;
        MainContractSupplierName: string | null;
        MainContractIsActive: boolean | null;
        SensitiveDataLevelsAsCsv: string | null;
        ArchiveDuty: string | null;
        LifeCycleStatus: string | null;
        IsHoldingDocument: boolean;
        ActiveArchivePeriodEndDate: Date | null;
        RiskSupervisionDocumentationName: string | null;
        RiskSupervisionDocumentationUrl: string | null;
        RiskAssessmentDate: Date | null;
        PlannedRiskAssessmentDate: Date | null;
        LinkToDirectoryName: string | null;
        LinkToDirectoryUrl: string | null;
        DataProcessingRegistrationsConcludedAsCsv: string | null;
        DataProcessingRegistrationNamesAsCsv: string | null;
        DataProcessingRegistrations: IItSystemUsageOverviewDataProcessingRegistrationReadModel[];
        GeneralPurpose: string | null;
        HostedAt: number | null;
        DependsOnInterfacesNamesAsCsv: string | null;
        DependsOnInterfaces: IItSystemUsageOverviewInterfaceReadModel[];
        IncomingRelatedItSystemUsagesNamesAsCsv: string | null;
        IncomingRelatedItSystemUsages: IItSystemUsageOverviewItSystemUsageReadModel[];
        OutgoingRelatedItSystemUsagesNamesAsCsv: string | null;
        OutgoingRelatedItSystemUsages: IItSystemUsageOverviewItSystemUsageReadModel[];
        RelevantOrganizationUnitNamesAsCsv: string;
        RelevantOrganizationUnits: IItSystemUsageOverviewRelevantOrgUnitReadModel[];
    }
}