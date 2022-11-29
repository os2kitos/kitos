module Kitos.Helpers {

    export interface IConvertedDateString {
        errorMessage?: string;
        convertedValue?: string;
    }

    export class DateStringFormat {

        static fromDDMMYYYYToYYYYMMDD(dateString: string): IConvertedDateString {
            var asMoment = moment(dateString, Constants.DateFormat.DanishDateFormat, true);
            if (asMoment.isValid()) {
                return { convertedValue: asMoment.format("YYYY-MM-DD") };
            }
            return { errorMessage: "Ugyldigt dato format" };
        }

        static fromDanishToEnglishFormat(dateString: string): string {
            return moment(dateString, [Kitos.Constants.DateFormat.DanishDateFormat, Kitos.Constants.DateFormat.EnglishDateFormat]).format(Constants.DateFormat.EnglishDateFormat);

        }
    }
}