module Kitos.Models.ViewModel.Generic {

    // Select 2 always returns selected value as string
    export type ElementSelectedFunc<T> = (element: T) => void;

    export interface ISingleSelectionWithFixedOptionsViewModel<TElement> {
        selectedElement: Generic.Select2OptionViewModel<TElement>;
        select2Config: any;
        elementSelected: ElementSelectedFunc<Generic.Select2OptionViewModel<TElement>>;
    }
}