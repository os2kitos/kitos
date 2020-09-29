module Kitos.Models.ViewModel.Shared {
    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export class YesNoIrrelevantOptions {

        private static getValueToTextMap() {
            return Object
                .keys(Models.Api.Shared.YesNoIrrelevantOption)
                .filter(k => isNaN(parseInt(k)) === false)
                .reduce((acc, next, _) => {
                        var text = "";

                        switch (parseInt(next) as Models.Api.Shared.YesNoIrrelevantOption) {
                        case Models.Api.Shared.YesNoIrrelevantOption.YES:
                            text = "Ja";
                            break;
                        case Models.Api.Shared.YesNoIrrelevantOption.NO:
                            text = "Nej";
                            break;
                        case Models.Api.Shared.YesNoIrrelevantOption.IRRELEVANT:
                            text = "Ikke relevant";
                            break;
                        }

                        //Set by numeric and text value
                        acc[next] = text;
                        acc[Models.Api.Shared.YesNoIrrelevantOption[next]] = text;
                        return acc;
                    },
                    {}
                );
        }

        //Cache the names for quick lookup
        private static readonly valueToNameMap = YesNoIrrelevantOptions.getValueToTextMap();

        static getText(option: Models.Api.Shared.YesNoIrrelevantOption) {
            return YesNoIrrelevantOptions.valueToNameMap[option];
        }

        options: Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption>[];
        constructor() {
            const select2BlankOptionTextValue = "\u200B";

            this.options = [
                <Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption>>{ id: Models.Api.Shared.YesNoIrrelevantOption.UNDECIDED as number, text: select2BlankOptionTextValue, optionalObjectContext: Models.Api.Shared.YesNoIrrelevantOption.UNDECIDED },
                <Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption>>{ id: Models.Api.Shared.YesNoIrrelevantOption.YES as number, text: YesNoIrrelevantOptions.getText(Models.Api.Shared.YesNoIrrelevantOption.YES), optionalObjectContext: Models.Api.Shared.YesNoIrrelevantOption.YES },
                <Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption>>{ id: Models.Api.Shared.YesNoIrrelevantOption.NO as number, text: YesNoIrrelevantOptions.getText(Models.Api.Shared.YesNoIrrelevantOption.NO), optionalObjectContext: Models.Api.Shared.YesNoIrrelevantOption.NO },
                <Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption>>{ id: Models.Api.Shared.YesNoIrrelevantOption.IRRELEVANT as number, text: YesNoIrrelevantOptions.getText(Models.Api.Shared.YesNoIrrelevantOption.IRRELEVANT), optionalObjectContext: Models.Api.Shared.YesNoIrrelevantOption.IRRELEVANT }
            ];
        }
    }
}