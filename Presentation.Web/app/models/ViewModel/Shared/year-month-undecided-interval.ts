module Kitos.Models.ViewModel.Shared {

    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export class YearMonthUndecidedIntervalOption {

        private static getValueToTextMap() {
            return Object
                .keys(Models.Api.Shared.YearMonthUndecidedIntervalOption)
                .filter(k => isNaN(parseInt(k)) === false)
                .reduce((acc, next, _) => {
                    var text = "";

                    switch (parseInt(next) as Models.Api.Shared.YearMonthUndecidedIntervalOption) {
                        case Models.Api.Shared.YearMonthUndecidedIntervalOption.Half_yearly:
                            text = "Halvårlig";
                            break;
                        case Models.Api.Shared.YearMonthUndecidedIntervalOption.Yearly:
                            text = "Årligt";
                            break;
                        case Models.Api.Shared.YearMonthUndecidedIntervalOption.Every_second_year:
                            text = "Hver andet år";
                            break;
                        case Models.Api.Shared.YearMonthUndecidedIntervalOption.Other:
                            text = "Andet";
                            break;
                    }

                    acc[next] = text;
                    acc[Models.Api.Shared.YearMonthUndecidedIntervalOption[next]] = text;
                    return acc;
                },
                    {}
                );
        }

        private static readonly valueToNameMap = YearMonthUndecidedIntervalOption.getValueToTextMap();

        static getText(option: Models.Api.Shared.YearMonthUndecidedIntervalOption) {
            return YearMonthUndecidedIntervalOption.valueToNameMap[option];
        }

        options: Select2OptionViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>[];

        getById(id: number): Select2OptionViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption> {
            if (id === null) {
                return null;
            }
            return this.options.filter(x => x.id === id)[0];
        }

        constructor() {

            this.options = [
                Models.Api.Shared.YearMonthUndecidedIntervalOption.Undecided,
                Models.Api.Shared.YearMonthUndecidedIntervalOption.Half_yearly,
                Models.Api.Shared.YearMonthUndecidedIntervalOption.Yearly,
                Models.Api.Shared.YearMonthUndecidedIntervalOption.Every_second_year,
                Models.Api.Shared.YearMonthUndecidedIntervalOption.Other
            ].map(optionType => {
                return <Select2OptionViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>>{
                    id: optionType as number,
                    text: optionType === Models.Api.Shared.YearMonthUndecidedIntervalOption.Undecided
                        ? ViewModel.Generic.select2BlankOptionTextValue
                        : YearMonthUndecidedIntervalOption.getText(optionType),
                    optionalObjectContext: optionType
                }
            });
        }
    }
}