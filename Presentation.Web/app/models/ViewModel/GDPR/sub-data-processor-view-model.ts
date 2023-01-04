module Kitos.Models.ViewModel.GDPR {

    export class SubDataProcessorViewModel {

        subDataProcessorId: number;
        basisForTransferId: number;
        transferToInsecureThirdCountryId: number;
        insecureCountryId: number;

        prepareRequestPayload(organizationId: number) {
            return {
                organizationId: organizationId,
                details: {
                    basisForTransferOptionId: this.basisForTransferId,
                    transferToInsecureThirdCountries: this.transferToInsecureThirdCountryId,
                    insecureCountryOptionId: this.insecureCountryId
                }
            } as Models.DataProcessing.IAssignSubDataProcessorRequestDTO;
        }
    }
}
