module Kitos.Models.DataProcessing {
    /** Contains info about Advices on a contract. */
    export interface IDataProcessingAgreement {
        Id: number;
        SourceEntityId : number;
        Name: string;
    }
}