module Kitos.Models.ViewModel.Shared {
    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export class YesNoIrrelevantOptions {

        static getText(option: Models.Api.Shared.YesNoIrrelevantOption) {
            switch (option) {
                case Models.Api.Shared.YesNoIrrelevantOption.YES:
                    return "Ja";
                case Models.Api.Shared.YesNoIrrelevantOption.NO:
                    return "Nej"
                case Models.Api.Shared.YesNoIrrelevantOption.IRRELEVANT:
                    return "Ikke relevant";
                default:
                    return "";
            }
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