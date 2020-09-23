module Kitos.Models.ViewModel.Generic {

    export type ElementSelectedFunc = (element: Generic.Select2OptionViewModel) => void;

    export interface ISingleSelectionWithFixedOptionsViewModel<TElement> {
        selectedElement: TElement;
        select2Options: Select2OptionViewModel[];
        elementSelected: ElementSelectedFunc;
    }
}