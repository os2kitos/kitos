module Kitos.Models.ViewModel.Generic {

    export interface ISelectedValueViewModel<T> {
        value: T;
        text: string;
        textValue: string;
    }
}