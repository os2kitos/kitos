module Kitos.Models.ViewModel.Generic {

    export const select2BlankOptionTextValue = "\u200B";

    export interface ISelect2Model {
        id: string;
        text: string;
    }

    export interface Select2OptionViewModel<T> {
        id: number;
        text: string;
        optionalObjectContext?: T;
        disabled?: boolean;
    }

    export interface UpdatedSelect2OptionViewModel<T> extends ISelect2Model {
        optionalObjectContext?: T;
        disabled?: boolean;
    }

    export interface Select2OptionViewModelWithIndentation<T> extends ISelect2Model {
        indentationLevel: number;
        optionalExtraObject?: T;
    }
}