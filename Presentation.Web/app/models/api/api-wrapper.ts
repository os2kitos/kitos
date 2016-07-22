module Kitos.API.Models {
    export interface IApiWrapper<T> {
        msg: string;
        response: T;
    }
}
