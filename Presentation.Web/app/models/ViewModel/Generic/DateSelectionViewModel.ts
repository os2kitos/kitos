module Kitos.Models.ViewModel.Generic {
    export interface IDateSelectionViewModel {
        selectedDate: string;
        dateFormat: IDateFormat;
        dateSelected: DateSelectedFunc;
    }

    export interface IDateFormat {
        format: string;
    }

    export type DateSelectedFunc = (date: string) => void;

    export class DateSelectionViewModel implements IDateSelectionViewModel {
        selectedDate: string;
        dateSelected: DateSelectedFunc;
        constructor(selectedDate: string, dateSelected: DateSelectedFunc) {
            this.selectedDate = selectedDate;
            this.dateSelected = dateSelected;
        }

        dateFormat = {
            format: "dd-MM-yyyy"
        };
    }
}