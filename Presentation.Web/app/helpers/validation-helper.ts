module Kitos.Helpers {
    export class ValidationHelper {
        static checkIfStartDateIsGreaterThanEndDate(startDate: moment.Moment, endDate: moment.Moment, notify): boolean {
            if (startDate >= endDate) {
                notify.addErrorMessage("Den indtastede slutdato er før startdatoen.");
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