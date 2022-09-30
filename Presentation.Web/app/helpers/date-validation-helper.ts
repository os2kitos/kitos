module Kitos.Helpers {
    export class DateValidationHelper {
        static checkIfStartDateIsGreaterThanEndDate(startDate: moment.Moment, endDate: moment.Moment, notify, startDateFieldName: string, endDateFieldName: string): boolean {
            if (startDate >= endDate) {
                notify.addErrorMessage(`Den indtastede ${endDateFieldName} er før ${startDateFieldName}.`);
                return true;
            }

            return false;
        }

        static checkIfDateIsInvalid(date: moment.Moment, notify): boolean {
            if (!date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                notify.addErrorMessage("Den indtastede dato er ugyldig.");
                return true;
            }

            return false;
        }
    }
}