module Kitos.Helpers {
    export class DateValidationHelper {
        static validateValidityPeriod(fromDate: string, endDate: string, notify, fromDateFieldName: string, endDateFieldName: string) {
            const formatDateString = Kitos.Constants.DateFormat.EnglishDateFormat;
            const formattedFromDate = fromDate ? moment(fromDate, [Kitos.Constants.DateFormat.DanishDateFormat, formatDateString]).startOf("day") : null;
            const formattedEndDate = endDate ? moment(endDate, [Kitos.Constants.DateFormat.DanishDateFormat, formatDateString]).endOf("day") : null;

            if (formattedEndDate == null && formattedFromDate == null) {
                return true;
            }
            if (formattedEndDate == null && formattedFromDate != null) {
                return this.checkIfDateIsValid(formattedFromDate, notify, fromDateFieldName);
            }
            if (formattedEndDate != null && formattedFromDate == null) {
                return this.checkIfDateIsValid(formattedEndDate, notify, endDateFieldName);
            }
            return DateValidationHelper.checkIfDateIsValid(formattedFromDate, notify, fromDateFieldName)
                && DateValidationHelper.checkIfDateIsValid(formattedEndDate, notify, endDateFieldName)
                && DateValidationHelper.checkIfStartDateIsSmallerThanEndDate(formattedFromDate,
                    formattedEndDate,
                    notify,
                    fromDateFieldName,
                    endDateFieldName);
        }

        static validateDateInput(date: string, notify, fieldName: string, emptyDateIsValid: boolean) {
            if (!date && emptyDateIsValid) {
                return true;
            }
            
            const formatDateString = Kitos.Constants.DateFormat.EnglishDateFormat;
            const formattedDate = moment(date, [Kitos.Constants.DateFormat.DanishDateFormat, formatDateString]);

            return DateValidationHelper.checkIfDateIsValid(formattedDate, notify, fieldName);
        }

        static checkIfStartDateIsSmallerThanEndDate(startDate: moment.Moment, endDate: moment.Moment, notify, startDateFieldName: string, endDateFieldName: string): boolean {
            if (startDate > endDate) {
                notify.addErrorMessage(`Den indtastede \"${endDateFieldName}\" er før \"${startDateFieldName}\".`);
                return false;
            }

            return true;
        }

        static checkIfDateIsValid(date: moment.Moment, notify, fieldName: string): boolean {
            if (date && !date.isValid() || isNaN(date.valueOf()) || date.year() < 1000 || date.year() > 2099) {
                notify.addErrorMessage(`Den indtastede  \"${fieldName}\" er ugyldig.`);
                return false;
            }

            return true;
        }
    }
}