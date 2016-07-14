module Kitos.Models {
    export interface IApiWrapper<T> {
        msg: string;
        response: T;
    }
}
