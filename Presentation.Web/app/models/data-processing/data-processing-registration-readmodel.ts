﻿module Kitos.Models.DataProcessing {

    export interface IAssignedDataProcessingRegistrationRole {
        RoleId : number;
        UserFullName : string;
    }

    export interface IDataProcessingRegistration {
        Id: number;
        SourceEntityId : number;
        Name: string;
        RoleAssignments: IAssignedDataProcessingRegistrationRole[];
        MainReferenceTitle: string;
        MainReferenceUrl: string;
        MainReferenceUserAssignedId: string;
        SystemNamesAsCsv: string;
        DataProcessorNamesAsCsv: string;
        SubDataProcessorNamesAsCsv: string;
        IsAgreementConcluded?: Models.Api.Shared.YesNoIrrelevantOption;
        AgreementConcludedAt?: string;
        LatestOversightDate?: string;
        TransferToInsecureThirdCountries?: Models.Api.Shared.YesNoUndecidedOption;
        BasisForTransfer?: string;
        OversightInterval?: Models.Api.Shared.YearMonthUndecidedIntervalOption;
        DataResponsible?: string;
        OversightOptionNamesAsCsv: string;
        IsOversightCompleted?: Models.Api.Shared.YesNoUndecidedOption;
        ContractNamesAsCsv: string;
        LastChangedAt: string;
        LastChangedByName: string;
        SystemUuidsAsCsv: string;
        OversightScheduledInspectionDate: string;
        ActiveAccordingToMainContract: boolean;
    }
}