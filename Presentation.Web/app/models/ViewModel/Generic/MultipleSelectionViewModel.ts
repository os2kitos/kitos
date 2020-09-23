module Kitos.Models.ViewModel.Generic {

    export type NewItemSelectedFunc = (element: Generic.Select2OptionViewModel) => void;

    export interface IMultipleSelectionWithSelect2ConfigViewModel<TElement> {
        selectedElements: TElement[];
        select2Config: any;
        newElementSelection? : Generic.Select2OptionViewModel;
        allowRemoval: boolean;
        allowAddition: boolean;
        removeItemRequested: (element: TElement) => void;
        newItemSelected: NewItemSelectedFunc;
    }
}