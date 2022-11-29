module Kitos.Models.ViewModel.Generic {

    export const select2BlankOptionTextValue = "\u200B";

    export interface ISelect2Model<TId> {
        id: TId;
        text: string;
    }

    export interface ISelect2ModelOptionalObjectContext<T> {
        optionalObjectContext?: T;
    }

    export interface ISelect2ModelItemState {
        disabled?: boolean;
    }

    export interface Select2OptionViewModel<T> extends ISelect2Model<number>, ISelect2ModelOptionalObjectContext<T>, ISelect2ModelItemState {
    }

    export interface UpdatedSelect2OptionViewModel<T> extends ISelect2Model<string>, ISelect2ModelOptionalObjectContext<T>, ISelect2ModelItemState {
    }

    export interface Select2OptionViewModelWithIndentation<T> extends ISelect2Model<string>, ISelect2ModelOptionalObjectContext<T>, ISelect2ModelItemState {
        indentationLevel: number;
    }
}