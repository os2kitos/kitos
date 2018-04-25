module Kitos.Models {
    export interface IODataResult<T> extends IODataMetadata {
        // if $count=true
        "@odata.count"?: number;
        value: T[];
    }

    export interface IODataSingleResult<T> extends IODataMetadata {
        // if $count=true
        "@odata.count"?: number;
        value: T;
    }

    export interface IODataMetadata {
        // odata.metadata=minimal
        "@odata.context"?: string;
        // odata.metadata=full
        "@odata.id": string,
        "@odata.etag": string,
        "@odata.editLink": string,
    }

    export interface IODataErrorResponse {
        error: {
            code: string,
            message: string,
            innererror: any,
        }
    }
}
