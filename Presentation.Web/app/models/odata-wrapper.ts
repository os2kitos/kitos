module Kitos.Models {
    export interface IOdataWrapper<T> {
        value: T;
        "@odata.context": string;
    }
}
