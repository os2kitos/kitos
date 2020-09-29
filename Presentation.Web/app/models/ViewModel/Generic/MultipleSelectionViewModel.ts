module Kitos.Models.ViewModel.Generic {
    export interface IMultipleSelectionWithSelect2ConfigViewModel<TElement> {
        selectedElements: TElement[];
        allowRemoval: boolean;
        allowAddition: boolean;
        removeItemRequested: (element: TElement) => void;
        newItemSelectionConfig: ISingleSelectionWithFixedOptionsViewModel<TElement>;
    }
}