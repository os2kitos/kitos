module Kitos.Models.DataProcessing {

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
        AgreementConcludedAt?: Date;
        OversightInterval?: Models.Api.Shared.YearMonthUndecidedIntervalOption;
    }
}