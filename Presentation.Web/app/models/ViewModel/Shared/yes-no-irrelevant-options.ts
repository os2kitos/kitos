module Kitos.Models.ViewModel.Shared {
    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export class YesNoIrrelevantOptions {
        options: Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption>[];
        constructor() {
            this.options = [
                <Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption>>{ id: 3, text: "\u200B", optionalObjectContext: Models.Api.Shared.YesNoIrrelevantOption.UNDECIDED },
                <Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption>>{ id: 1, text: "Ja", optionalObjectContext: Models.Api.Shared.YesNoIrrelevantOption.YES },
                <Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption>>{ id: 0, text: "Nej", optionalObjectContext: Models.Api.Shared.YesNoIrrelevantOption.NO },
                <Select2OptionViewModel<Models.Api.Shared.YesNoIrrelevantOption>>{ id: 2, text: "Ikke relevant", optionalObjectContext: Models.Api.Shared.YesNoIrrelevantOption.IRRELEVANT }
            ];
        }
    }
}