module Kitos.Models.Api.Shared {
    export enum YearMonthUndecidedIntervalOption {
        Half_yearly = 0,
        Yearly = 1,
        Every_second_year = 2,
        Other = 3,
        Undecided = 4
    }

    export class YearMonthUndecidedOptionMapper {
        static getValueToTextMap() {
            return Object
                .keys(YearMonthUndecidedIntervalOption)
                .filter(k => isNaN(parseInt(k)) === false)
                .reduce((acc, next, _) => {
                    const text = Models.ViewModel.Shared.YearMonthUndecidedIntervalOption.getText(parseInt(next));
                    acc[next] = text;
                    acc[YearMonthUndecidedIntervalOption[next]] = text;
                    return acc;
                },
                    {}
                );
        }
    }

}