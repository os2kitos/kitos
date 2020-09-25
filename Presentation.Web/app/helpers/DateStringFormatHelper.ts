module Kitos.Helpers {

    export interface IConvertedDateString {
        errorMessage?: string;
        convertedValue?: string;
    }

    export class DateStringFormat {

        static fromDDMMYYYYToYYYYMMDD(dateString: string): IConvertedDateString {
            var asMoment = moment(dateString, "DD-MM-YYYY", true);
            if (asMoment.isValid()) {
                return { convertedValue: asMoment.format("YYYY-MM-DD") };
            }
            return { errorMessage: "Ugyldigt dato format" };
        }
    }
}