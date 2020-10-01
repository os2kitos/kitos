module Kitos.Models.ViewModel.Generic {
    export interface IEditTextViewModel {
        currentText: string;
        newText: TextChanged;
    }

    export type TextChanged = (newText: string) => void;

    export class EditTextViewModel implements IEditTextViewModel {
        currentText: string;
        newText: TextChanged;
        constructor(currentText: string, newText: TextChanged) {
            if (newText === null) {
                throw new Error("newText is not set as a function");
            }
            this.currentText = currentText;
            this.newText = newText;
        }
    }
}