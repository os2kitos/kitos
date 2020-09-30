module Kitos.Models.ViewModel.Generic {
    export interface IEditTextViewModel {
        currentText: string;
        newText: NewTextAdded;
    }

    export type NewTextAdded = (newText: string) => void;

    export class EditTextViewModel implements IEditTextViewModel {
        currentText: string;
        newText: NewTextAdded;
        constructor(currentText: string, newText: NewTextAdded) {
            this.currentText = currentText;
            this.newText = newText;
        }
    }
}