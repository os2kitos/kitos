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

    export interface IDataProcessingAgreementViewModel {
        id: number;
        organizationId: number;
        isDataAgreementConcluded: AgreementConcludedOptions;
    }

    export class DataProcessingAgreementViewModel implements IDataProcessingAgreementViewModel{
        id: number;
        organizationId: number;
        isDataAgreementConcluded: AgreementConcludedOptions;

        constructor(dataProcessingAgreementDTO: Models.DataProcessing.IDataProcessingAgreementDTO) {
            this.id = dataProcessingAgreementDTO.id;
            this.isDataAgreementConcluded
        }
    }

}