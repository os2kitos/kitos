module Kitos.Models.OData.DataProcessing {
    export interface IDataProcessingRegistration {
        Id: number;
        Name: string;
        IsAgreementConcluded?: Models.Api.Shared.YesNoIrrelevantOption;
    }
}