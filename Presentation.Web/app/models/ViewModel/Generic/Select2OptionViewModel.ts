module Kitos.Models.ViewModel.Generic {

    export const select2BlankOptionTextValue = "\u200B";

    export interface Select2OptionViewModel<T> {
        id: number;
        text: string;
        optionalObjectContext?: T;
        disabled?: boolean;
    }

    export interface UpdatedSelect2OptionViewModel<T> {
        id: string;
        text: string;
        optionalObjectContext?: T;
        disabled?: boolean;
    }
}