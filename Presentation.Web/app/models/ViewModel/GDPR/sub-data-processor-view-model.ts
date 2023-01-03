module Kitos.Models.ViewModel.GDPR {

    export class SubDataProcessorViewModel {

        subDataProcessorConfig: ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.DataProcessing.IDataProcessorDTO>;
        transferToThirdCountryConfig: ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoUndecidedOption>;
        basisForTransferConfig: ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>;
        insecureThirdCountryConfig: ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>;
        isEdit: boolean;

        selectedSubDataProcessor: DataProcessing.IDataProcessorDTO;
        /*private readonly subDataProcessor: ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.DataProcessing.IDataProcessorDTO>,*/
        
        constructor(private readonly dpr: Models.DataProcessing.IDataProcessingRegistrationDTO,
            thirdCountryOptions: ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Api.Shared.YesNoUndecidedOption>,
            basisForTransferOptions: ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>,
            insecureThirdCountryOptions: ViewModel.Generic.ISingleSelectionWithFixedOptionsViewModel<Models.Generic.NamedEntity.NamedEntityWithDescriptionAndExpirationStatusDTO>) {
            /*this.subDataProcessorConfig = this.subDataProcessor;*/
            this.transferToThirdCountryConfig = thirdCountryOptions;
            this.basisForTransferConfig = basisForTransferOptions;
            this.insecureThirdCountryConfig = insecureThirdCountryOptions;
        }

        configureAsCreate() {
            this.isEdit = false;
        }

        configureAsEdit(subDprId: number) {
            this.isEdit = true;
            const matchingSubDataProcessors = this.dpr.subDataProcessors.filter(x => x.id === subDprId);
            if (matchingSubDataProcessors.length === 1) {
                this.selectedSubDataProcessor = matchingSubDataProcessors[0];
            }
        }

    }
}
