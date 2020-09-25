module Kitos.Models.ViewModel.Generic {
    export interface IDateSelectionViewModel {
        selectedDate: string;
        dateSelected: DateSelectedFunc;
    }

    export type DateSelectedFunc = (date: string) => void;

    export class DateSelectionViewModel implements IDateSelectionViewModel {
        selectedDate: string;
        dateSelected: DateSelectedFunc;
        constructor(selectedDate: string, dateSelected: DateSelectedFunc) {
            this.selectedDate = selectedDate;
            this.dateSelected = dateSelected;
        }
    }
}