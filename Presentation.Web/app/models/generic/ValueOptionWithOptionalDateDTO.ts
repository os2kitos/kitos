module Kitos.Models.Generic {

    export interface ValueOptionWithOptionalDateDTO<T> {
        value: T;
        optionalDateValue? : string;
    }

}