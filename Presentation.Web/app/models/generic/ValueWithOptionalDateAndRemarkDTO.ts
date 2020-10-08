module Kitos.Models.Generic {

    export interface ValueWithOptionalDateAndRemarkDTO<T> {
        value: T;
        optionalDateValue?: string;
        remark: string;
    }

}