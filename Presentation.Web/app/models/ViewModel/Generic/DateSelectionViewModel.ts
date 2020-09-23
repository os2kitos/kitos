module Kitos.Models.ViewModel.Generic {
    export interface IDateSelectionViewModel {
        selectedDate: string;
        dateFormat: any;
        dateSelected: ElementSelectedFunc;
    }

    export class DateSelectionViewModel implements IDateSelectionViewModel {
        selectedDate: string;
        dateSelected: ElementSelectedFunc;
        constructor(selectedDate: string, dateSelected: ElementSelectedFunc) {
            this.selectedDate = selectedDate;
            this.dateSelected = dateSelected;
        }

        dateFormat = {
            format: "dd-MM-yyyy"
        };
    }
}