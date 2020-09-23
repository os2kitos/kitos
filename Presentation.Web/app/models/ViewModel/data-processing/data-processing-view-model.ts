module Kitos.Models.ViewModel.DataProcessingAgreement {
    import Select2OptionViewModel = ViewModel.Generic.Select2OptionViewModel;

    export class AgreementConcludedOptions {
        options: Select2OptionViewModel[];
        constructor() {
            this.options = [
                <Select2OptionViewModel>{ id: 3, text: "&nbsp" },
                <Select2OptionViewModel>{ id: 1, text: "Ja" },
                <Select2OptionViewModel>{ id: 0, text: "Nej" },
                <Select2OptionViewModel>{ id: 2, text: "Ikke relevant" }
            ];
        }
    }
}