module Kitos.Models.DataProcessing {

    export interface IAssignedDataProcessingAgreementRole {
        RoleId : number;
        UserFullName : string;
    }

    export interface IDataProcessingAgreement {
        Id: number;
        SourceEntityId : number;
        Name: string;
        RoleAssignments: IAssignedDataProcessingAgreementRole[];
        MainReferenceTitle: string;
        MainReferenceUrl: string;
        MainReferenceUserAssignedId: string;
        SystemNamesAsCsv: string;
    }
}