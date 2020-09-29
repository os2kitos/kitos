module Kitos.Models.ViewModel.Shared {
    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export class YesNoUndecidedOptions {

        private static getValueToTextMap() {
            return Object
                .keys(Models.Api.Shared.YesNoUndecidedOption)
                .filter(k => isNaN(parseInt(k)) === false)
                .reduce((acc, next, _) => {
                        var text = "";

                        switch (parseInt(next) as Models.Api.Shared.YesNoUndecidedOption) {
                        case Models.Api.Shared.YesNoUndecidedOption.Yes:
                            text = "Ja";
                            break;
                        case Models.Api.Shared.YesNoUndecidedOption.No:
                            text = "Nej";
                            break;
                        }

                        //Set by numeric and text value
                        acc[next] = text;
                        acc[Models.Api.Shared.YesNoUndecidedOption[next]] = text;
                        return acc;
                    },
                    {}
                );
        }

        //Cache texts for quick lookup texts
        private static readonly valueToTextMap = YesNoUndecidedOptions.getValueToTextMap();

        static getText(option: Models.Api.Shared.YesNoUndecidedOption) {
            return YesNoUndecidedOptions.valueToTextMap[option];
        }

        getById(id?: number): Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption>{
            if (id === null) {
                return null;
            }
            return this.options.filter(x => x.id === id)[0];
        }

        options: Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption>[];
        constructor() {
            const select2BlankOptionTextValue = "\u200B";

            this.options = [
                <Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption>>{ id: Models.Api.Shared.YesNoUndecidedOption.Undecided as number, text: select2BlankOptionTextValue, optionalObjectContext: Models.Api.Shared.YesNoUndecidedOption.Undecided },
                <Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption>>{ id: Models.Api.Shared.YesNoUndecidedOption.Yes as number, text: YesNoUndecidedOptions.getText(Models.Api.Shared.YesNoUndecidedOption.Yes), optionalObjectContext: Models.Api.Shared.YesNoUndecidedOption.Yes },
                <Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption>>{ id: Models.Api.Shared.YesNoUndecidedOption.No as number, text: YesNoUndecidedOptions.getText(Models.Api.Shared.YesNoUndecidedOption.No), optionalObjectContext: Models.Api.Shared.YesNoUndecidedOption.No }
            ];
        }
    }
}