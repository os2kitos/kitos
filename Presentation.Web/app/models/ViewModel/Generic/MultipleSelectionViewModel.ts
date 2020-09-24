module Kitos.Models.ViewModel.Generic {

    export type NewItemSelectedFunc = (element: Generic.Select2OptionViewModel<any>) => void;

    export interface IMultipleSelectionWithSelect2ConfigViewModel<TElement> {
        selectedElements: TElement[];
        select2Config: any;
        newElementSelection? : Generic.Select2OptionViewModel<any>;
        allowRemoval: boolean;
        allowAddition: boolean;
        removeItemRequested: (element: TElement) => void;
        newItemSelected: NewItemSelectedFunc;
    }
}