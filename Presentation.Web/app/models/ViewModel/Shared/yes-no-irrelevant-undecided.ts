module Kitos.Models.ViewModel.Shared {
    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export class YesNoUndecidedOptions {
        options: Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption>[];
        constructor() {
            this.options = [
                <Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption>>{ id: 2, text: "&nbsp", optionalObjectContext: Models.Api.Shared.YesNoUndecidedOption.Undecided },
                <Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption>>{ id: 0, text: "Ja", optionalObjectContext: Models.Api.Shared.YesNoUndecidedOption.Yes },
                <Select2OptionViewModel<Models.Api.Shared.YesNoUndecidedOption>>{ id: 1, text: "Nej", optionalObjectContext: Models.Api.Shared.YesNoUndecidedOption.No },
            ];
        }
    }
}