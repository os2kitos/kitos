module Kitos.Models.ViewModel.GDPR {

    export class SubDataProcessorViewModel {

        subDataProcessorId: number | null = null;
        cvrNumber: string | null = null;
        basisForTransferId: number | null = null;
        transferToInsecureThirdCountryId: number | null = null;
        insecureCountryId: number | null = null;

        constructor(subDataProcessorId: number | null,
            subDataProcessorCvr: string | null,
            basisForTransferId: number | null,
            transferToInsecureThirdCountryId: number | null,
            insecureCountryId: number | null) {

            this.subDataProcessorId = subDataProcessorId;
            this.cvrNumber = subDataProcessorCvr;
            this.basisForTransferId = basisForTransferId;
            this.transferToInsecureThirdCountryId = transferToInsecureThirdCountryId;
            this.insecureCountryId = insecureCountryId;
        }

        prepareRequestPayload() {
            return {
                organizationId: this.subDataProcessorId,
                details: {
                    basisForTransferOptionId: this.basisForTransferId,
                    transferToInsecureThirdCountries: this.transferToInsecureThirdCountryId,
                    insecureCountryOptionId: this.transferToInsecureThirdCountryId === Api.Shared.YesNoUndecidedOption.Yes ? this.insecureCountryId : null
                }
            } as Models.DataProcessing.ISubDataProcessorRequestDTO;
        }
    }
}
