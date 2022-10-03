module Kitos.Helpers {
    export class DateValidationHelper {
        static validateInterval(startDate:string , endDate: string, notify, startDateFieldName: string, endDateFieldName: string) {
            const formatDateString = "YYYY-MM-DD";
            const formattedFromDate = startDate != null ? moment(startDate, [Kitos.Constants.DateFormat.DanishDateFormat, formatDateString]).startOf("day") : null;
            const formattedEndDate = endDate != null ? moment(endDate, [Kitos.Constants.DateFormat.DanishDateFormat, formatDateString]).endOf("day") : null;

            return DateValidationHelper.checkIfDateIsValid(formattedFromDate, notify)
                && DateValidationHelper.checkIfDateIsValid(formattedEndDate, notify)
                && DateValidationHelper.checkIfStartDateIsSmallerThanEndDate(formattedFromDate,
                    formattedEndDate,
                    notify,
                    startDateFieldName,
                    endDateFieldName);
        }

        static checkIfStartDateIsSmallerThanEndDate(startDate: moment.Moment, endDate: moment.Moment, notify, startDateFieldName: string, endDateFieldName: string): boolean {
            if (startDate > endDate) {
                notify.addErrorMessage(`Den indtastede \"${endDateFieldName}\" er før \"${startDateFieldName}\".`);
                return false;
            }

            return true;
        }

        static checkIfDateIsValid(date: moment.Moment, notify): boolean {
            if (date != null && !date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                notify.addErrorMessage("Den indtastede dato er ugyldig.");
                return false;
            }

            return true;
        }
    }
}