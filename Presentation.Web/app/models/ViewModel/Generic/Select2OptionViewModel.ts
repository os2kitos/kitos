module Kitos.Models.ViewModel.Generic {

    export const select2BlankOptionTextValue = "\u200B";

    export interface Select2OptionViewModel<T> {
        id: number;
        text: string;
        optionalObjectContext?: T;
        disabled?: boolean;
    }
}