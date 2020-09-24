module Kitos.Models.ViewModel.Generic {

    export interface Select2OptionViewModel<T> {
        id: number;
        text: string;
        optionalObjectContext? : T;
    }
}