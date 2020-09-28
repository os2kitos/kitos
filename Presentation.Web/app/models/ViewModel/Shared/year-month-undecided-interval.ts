module Kitos.Models.ViewModel.Shared {
    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export class YearMonthUndecidedIntervalOption {

        static getText(option: Models.Api.Shared.YearMonthUndecidedIntervalOption) {
            switch (option) {
                case Models.Api.Shared.YearMonthUndecidedIntervalOption.Half_yearly:
                    return "Halvårlig";
                case Models.Api.Shared.YearMonthUndecidedIntervalOption.Yearly:
                    return "Årligt";
                case Models.Api.Shared.YearMonthUndecidedIntervalOption.Every_second_year:
                    return "Hver andet år";
                case Models.Api.Shared.YearMonthUndecidedIntervalOption.Other:
                    return "Andet";
                case Models.Api.Shared.YearMonthUndecidedIntervalOption.Undecided:
                    return "Ikke besluttet";
                default:
                    return "";
            }
        }

        options: Select2OptionViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>[];
        constructor() {
            const select2BlankOptionTextValue = "\u200B";

            this.options = [
                <Select2OptionViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>>{ id: Models.Api.Shared.YearMonthUndecidedIntervalOption.Half_yearly as number, text: YearMonthUndecidedIntervalOption.getText(Models.Api.Shared.YearMonthUndecidedIntervalOption.Half_yearly), optionalObjectContext: Models.Api.Shared.YearMonthUndecidedIntervalOption.Half_yearly },
                <Select2OptionViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>>{ id: Models.Api.Shared.YearMonthUndecidedIntervalOption.Yearly as number, text: YearMonthUndecidedIntervalOption.getText(Models.Api.Shared.YearMonthUndecidedIntervalOption.Yearly), optionalObjectContext: Models.Api.Shared.YearMonthUndecidedIntervalOption.Yearly },
                <Select2OptionViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>>{ id: Models.Api.Shared.YearMonthUndecidedIntervalOption.Every_second_year as number, text: YearMonthUndecidedIntervalOption.getText(Models.Api.Shared.YearMonthUndecidedIntervalOption.Every_second_year), optionalObjectContext: Models.Api.Shared.YearMonthUndecidedIntervalOption.Every_second_year },
                <Select2OptionViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>>{ id: Models.Api.Shared.YearMonthUndecidedIntervalOption.Other as number, text: YearMonthUndecidedIntervalOption.getText(Models.Api.Shared.YearMonthUndecidedIntervalOption.Other), optionalObjectContext: Models.Api.Shared.YearMonthUndecidedIntervalOption.Other },
                <Select2OptionViewModel<Models.Api.Shared.YearMonthUndecidedIntervalOption>>{ id: Models.Api.Shared.YearMonthUndecidedIntervalOption.Undecided as number, text: select2BlankOptionTextValue, optionalObjectContext: Models.Api.Shared.YearMonthUndecidedIntervalOption.Undecided }
            ];
        }
    }
}