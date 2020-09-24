module Kitos.Models.ViewModel.Generic {

    // Select 2 always returns selected value as string
    export type ElementSelectedFunc = (element: string) => void;

    export interface ISingleSelectionWithFixedOptionsViewModel<TElement> {
        selectedElement: TElement;
        select2Options: Select2OptionViewModel<TElement>[];
        elementSelected: ElementSelectedFunc;
    }
}